using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace ShopTechnology.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            var apiKey = configuration["EmailSettings:SendGridApiKey"];
            _sendGridClient = new SendGridClient(apiKey);
            _fromEmail = configuration["EmailSettings:FromEmail"] ?? "noreply@shoptechnology.com";
            _fromName = configuration["EmailSettings:FromName"] ?? "Shop Technology";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var from = new EmailAddress(_fromEmail, _fromName);
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, isHtml ? null : body, isHtml ? body : null);
                var response = await _sendGridClient.SendEmailAsync(msg);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendEmailConfirmationAsync(string to, string callbackUrl)
        {
            var subject = "Confirm your email address";
            var body = $@"
                <h2>Welcome to Shop Technology!</h2>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{callbackUrl}'>Confirm Email</a></p>
                <p>If you didn't create an account, you can safely ignore this email.</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendPasswordResetAsync(string to, string callbackUrl)
        {
            var subject = "Reset your password";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to set a new password:</p>
                <p><a href='{callbackUrl}'>Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                <p>This link will expire in 1 hour.</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, string orderDetails)
        {
            var subject = $"Order Confirmation - {orderNumber}";
            var body = $@"
                <h2>Thank you for your order!</h2>
                <p>Your order has been confirmed and is being processed.</p>
                <p><strong>Order Number:</strong> {orderNumber}</p>
                <div>{orderDetails}</div>
                <p>We'll send you updates as your order progresses.</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendOrderStatusUpdateAsync(string to, string orderNumber, string status, string? trackingNumber = null)
        {
            var subject = $"Order Status Update - {orderNumber}";
            var body = $@"
                <h2>Order Status Update</h2>
                <p><strong>Order Number:</strong> {orderNumber}</p>
                <p><strong>Status:</strong> {status}</p>";

            if (!string.IsNullOrEmpty(trackingNumber))
            {
                body += $@"<p><strong>Tracking Number:</strong> {trackingNumber}</p>";
            }

            body += "<p>Thank you for shopping with us!</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to Shop Technology!";
            var body = $@"
                <h2>Welcome to Shop Technology, {userName}!</h2>
                <p>Thank you for joining our community. We're excited to have you on board!</p>
                <p>Here's what you can do:</p>
                <ul>
                    <li>Browse our latest products</li>
                    <li>Save items to your wishlist</li>
                    <li>Track your orders</li>
                    <li>Write reviews</li>
                </ul>
                <p>Happy shopping!</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendPromotionEmailAsync(string to, string subject, string promotionDetails)
        {
            var body = $@"
                <h2>Special Offer Just for You!</h2>
                <div>{promotionDetails}</div>
                <p>Don't miss out on these amazing deals!</p>";

            return await SendEmailAsync(to, subject, body, true);
        }

        public async Task<bool> SendStockAlertAsync(string to, string productName, int currentStock)
        {
            var subject = "Low Stock Alert";
            var body = $@"
                <h2>Low Stock Alert</h2>
                <p>The following product is running low on stock:</p>
                <p><strong>Product:</strong> {productName}</p>
                <p><strong>Current Stock:</strong> {currentStock}</p>
                <p>Please restock this item soon.</p>";

            return await SendEmailAsync(to, subject, body, true);
        }
    }
}
