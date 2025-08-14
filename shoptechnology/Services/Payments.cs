using Microsoft.Extensions.Configuration;

namespace ShopTechnologyAccessories.Services;

public interface IPaymentService
{
    Task<string> CreatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl);
    Task<bool> CapturePaymentAsync(string paymentId, string payerId);
}

public class PayPalService : IPaymentService
{
    private readonly IConfiguration _configuration;
    public PayPalService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> CreatePaymentAsync(decimal amount, string currency, string returnUrl, string cancelUrl)
    {
        // Stub: integrate PayPal SDK here
        var fakeCheckoutUrl = $"/Checkout/PayPalReturn?paymentId=fake&token=fake&payerId=fake";
        return Task.FromResult(fakeCheckoutUrl);
    }

    public Task<bool> CapturePaymentAsync(string paymentId, string payerId)
    {
        // Stub capture
        return Task.FromResult(true);
    }
}

public class VNPayService
{
    private readonly IConfiguration _configuration;
    public VNPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> CreatePaymentUrlAsync(decimal amount, string returnUrl)
    {
        // Stub: generate VNPay URL
        var url = $"/Checkout/VNPayReturn?vnp_ResponseCode=00&vnp_TxnRef=fake";
        return Task.FromResult(url);
    }
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody);
}

public class NoOpEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody)
    {
        return Task.CompletedTask;
    }
}


