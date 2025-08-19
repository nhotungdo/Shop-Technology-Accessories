using System.Net.Mail;
using System.Net;
using System.Text;
using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOrderConfirmationAsync(OrderDTO order)
    {
        var subject = $"Xác nhận đơn hàng #{order.OrderId}";
        var body = GenerateOrderConfirmationEmail(order);

        return await SendEmailAsync(order.UserEmail, subject, body);
    }

    public async Task<bool> SendOrderStatusUpdateAsync(OrderDTO order, string newStatus)
    {
        var subject = $"Cập nhật trạng thái đơn hàng #{order.OrderId}";
        var body = GenerateOrderStatusUpdateEmail(order, newStatus);

        return await SendEmailAsync(order.UserEmail, subject, body);
    }

    public async Task<bool> SendPasswordResetAsync(string email, string resetToken)
    {
        var subject = "Đặt lại mật khẩu - Shop Technology";
        var body = GeneratePasswordResetEmail(resetToken);

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
    {
        return await SendPasswordResetAsync(email, resetToken);
    }

    public async Task<bool> SendWelcomeEmailAsync(UserDTO user)
    {
        var subject = "Chào mừng đến với Shop Technology!";
        var body = GenerateWelcomeEmail(user);

        return await SendEmailAsync(user.Email, subject, body);
    }

    public async Task<bool> SendPromotionEmailAsync(string email, string subject, string body)
    {
        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendReviewReminderAsync(OrderDTO order)
    {
        var subject = "Đánh giá sản phẩm - Shop Technology";
        var body = GenerateReviewReminderEmail(order);

        return await SendEmailAsync(order.UserEmail, subject, body);
    }

    public async Task<bool> SendLowStockAlertAsync(ProductDTO product)
    {
        var subject = $"Cảnh báo: Sản phẩm {product.ProductName} sắp hết hàng";
        var body = GenerateLowStockAlertEmail(product);

        // Send to admin email
        var adminEmail = _configuration["Email:AdminEmail"] ?? "admin@shoptech.com";
        return await SendEmailAsync(adminEmail, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("Email:Smtp");
            var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@shoptech.com";
            var fromName = _configuration["Email:FromName"] ?? "Shop Technology";

            using var client = new SmtpClient(smtpSettings["Host"])
            {
                Port = int.Parse(smtpSettings["Port"] ?? "587"),
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);

            await client.SendMailAsync(message);

            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}");
            return false;
        }
    }

    private string GenerateOrderConfirmationEmail(OrderDTO order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Xác nhận đơn hàng</h2>");
        sb.AppendLine($"<p>Xin chào {order.UserFullName},</p>");
        sb.AppendLine($"<p>Cảm ơn bạn đã đặt hàng tại Shop Technology. Dưới đây là thông tin đơn hàng của bạn:</p>");
        sb.AppendLine($"<p><strong>Mã đơn hàng:</strong> {order.OrderId}</p>");
        sb.AppendLine($"<p><strong>Ngày đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>");
        sb.AppendLine($"<p><strong>Tổng tiền:</strong> {order.TotalAmount:N0} VNĐ</p>");
        sb.AppendLine($"<p><strong>Trạng thái:</strong> {order.StatusDisplay}</p>");
        sb.AppendLine("<p>Chúng tôi sẽ thông báo khi đơn hàng được xử lý.</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GenerateOrderStatusUpdateEmail(OrderDTO order, string newStatus)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Cập nhật trạng thái đơn hàng</h2>");
        sb.AppendLine($"<p>Xin chào {order.UserFullName},</p>");
        sb.AppendLine($"<p>Đơn hàng #{order.OrderId} của bạn đã được cập nhật trạng thái:</p>");
        sb.AppendLine($"<p><strong>Trạng thái mới:</strong> {newStatus}</p>");
        sb.AppendLine("<p>Bạn có thể theo dõi đơn hàng trong tài khoản của mình.</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GeneratePasswordResetEmail(string resetToken)
    {
        var resetUrl = $"{_configuration["AppUrl"]}/Account/ResetPassword?token={resetToken}";

        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Đặt lại mật khẩu</h2>");
        sb.AppendLine("<p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Shop Technology.</p>");
        sb.AppendLine($"<p>Vui lòng click vào link sau để đặt lại mật khẩu:</p>");
        sb.AppendLine($"<p><a href='{resetUrl}'>{resetUrl}</a></p>");
        sb.AppendLine("<p>Link này có hiệu lực trong 24 giờ.</p>");
        sb.AppendLine("<p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GenerateWelcomeEmail(UserDTO user)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Chào mừng đến với Shop Technology!</h2>");
        sb.AppendLine($"<p>Xin chào {user.FullName},</p>");
        sb.AppendLine("<p>Cảm ơn bạn đã đăng ký tài khoản tại Shop Technology.</p>");
        sb.AppendLine("<p>Bây giờ bạn có thể:</p>");
        sb.AppendLine("<ul>");
        sb.AppendLine("<li>Mua sắm các sản phẩm công nghệ chất lượng</li>");
        sb.AppendLine("<li>Theo dõi đơn hàng của mình</li>");
        sb.AppendLine("<li>Đánh giá sản phẩm đã mua</li>");
        sb.AppendLine("<li>Nhận thông báo khuyến mãi</li>");
        sb.AppendLine("</ul>");
        sb.AppendLine("<p>Chúc bạn có trải nghiệm mua sắm tuyệt vời!</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GenerateReviewReminderEmail(OrderDTO order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Đánh giá sản phẩm</h2>");
        sb.AppendLine($"<p>Xin chào {order.UserFullName},</p>");
        sb.AppendLine($"<p>Cảm ơn bạn đã mua hàng tại Shop Technology. Chúng tôi rất mong nhận được đánh giá của bạn về các sản phẩm trong đơn hàng #{order.OrderId}.</p>");
        sb.AppendLine("<p>Đánh giá của bạn sẽ giúp chúng tôi cải thiện dịch vụ và giúp khách hàng khác có quyết định mua hàng tốt hơn.</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology Team</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string GenerateLowStockAlertEmail(ProductDTO product)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine("<h2>Cảnh báo: Sản phẩm sắp hết hàng</h2>");
        sb.AppendLine($"<p>Sản phẩm <strong>{product.ProductName}</strong> chỉ còn {product.StockQuantity} sản phẩm trong kho.</p>");
        sb.AppendLine("<p>Vui lòng kiểm tra và bổ sung hàng tồn kho nếu cần thiết.</p>");
        sb.AppendLine("<p>Trân trọng,<br>Shop Technology System</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
