using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShopTechnologyAccessories.Models;
using ShopTechnologyAccessories.Services;
using System.Security.Claims;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ShopTechnologyAccessoriesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    // Use role-based requirement to avoid case/claim-type mismatches
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Application services
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<IPaymentService, PayPalService>();
builder.Services.AddScoped<VNPayService>(); // Register VNPay concrete as well if needed
builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();

var app = builder.Build();

// Seed basic roles and an admin account
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopTechnologyAccessoriesContext>();
    db.Database.EnsureCreated();

    if (!db.Roles.Any())
    {
        db.Roles.AddRange(new Role { RoleName = "Admin" }, new Role { RoleName = "User" });
        db.SaveChanges();
    }

    // Ensure there is at least one admin and also ensure the requested testing account exists
    var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    var adminRoleId = db.Roles.First(r => r.RoleName == "Admin").RoleId;

    if (!db.Users.Any(u => u.Email == "donhotung2004@gmail.com"))
    {
        db.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Admin",
            Email = "donhotung2004@gmail.com",
            PasswordHash = hasher.HashPassword("123456"),
            RoleId = adminRoleId,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    // Force-create or update the specific admin account provided by user for testing
    var targetEmail = "donhotung2004@gmail.com";
    var target = db.Users.FirstOrDefault(u => u.Email == targetEmail);
    if (target == null)
    {
        db.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Admin",
            Email = targetEmail,
            PasswordHash = hasher.HashPassword("123456"),
            RoleId = adminRoleId,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
    else
    {
        var changed = false;
        if (target.RoleId != adminRoleId) { target.RoleId = adminRoleId; changed = true; }
        // Reset password to the provided one to avoid mismatch
        target.PasswordHash = hasher.HashPassword("123456");
        target.UpdatedAt = DateTime.UtcNow;
        changed = true;
        if (changed) db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Ensure consistent number parsing from HTML number inputs (dot decimal)
var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
