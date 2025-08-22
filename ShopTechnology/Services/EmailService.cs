using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

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

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpServer = smtpSettings["Server"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            var smtpUsername = smtpSettings["Username"];
            var smtpPassword = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"] ?? smtpUsername;

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("SMTP credentials not configured");
                return false;
            }

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Shop Technology"),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var subject = "Đặt lại mật khẩu - Shop Technology";
        var body = $@"
            <html>
            <body>
                <h2>Đặt lại mật khẩu</h2>
                <p>Xin chào,</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Shop Technology.</p>
                <p>Vui lòng click vào link bên dưới để đặt lại mật khẩu:</p>
                <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a></p>
                <p>Hoặc copy link này vào trình duyệt: {resetLink}</p>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,<br>Shop Technology Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body, true);
    }

    public async Task<bool> SendOrderConfirmationEmailAsync(string email, string orderNumber, decimal totalAmount)
    {
        var subject = $"Xác nhận đơn hàng #{orderNumber} - Shop Technology";
        var body = $@"
            <html>
            <body>
                <h2>Xác nhận đơn hàng</h2>
                <p>Xin chào,</p>
                <p>Cảm ơn bạn đã đặt hàng tại Shop Technology!</p>
                <p><strong>Số đơn hàng:</strong> {orderNumber}</p>
                <p><strong>Tổng tiền:</strong> {totalAmount:N0} VNĐ</p>
                <p>Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.</p>
                <p>Bạn có thể theo dõi trạng thái đơn hàng trong tài khoản của mình.</p>
                <p>Trân trọng,<br>Shop Technology Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body, true);
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
    {
        var subject = "Chào mừng đến với Shop Technology!";
        var body = $@"
            <html>
            <body>
                <h2>Chào mừng bạn đến với Shop Technology!</h2>
                <p>Xin chào {fullName},</p>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại Shop Technology!</p>
                <p>Chúng tôi rất vui mừng được phục vụ bạn với những sản phẩm công nghệ chất lượng cao.</p>
                <p>Bạn có thể bắt đầu mua sắm ngay bây giờ!</p>
                <p>Trân trọng,<br>Shop Technology Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body, true);
    }

    public async Task<bool> SendOrderStatusUpdateEmailAsync(string email, string orderNumber, string status)
    {
        var subject = $"Cập nhật trạng thái đơn hàng #{orderNumber} - Shop Technology";
        var body = $@"
            <html>
            <body>
                <h2>Cập nhật trạng thái đơn hàng</h2>
                <p>Xin chào,</p>
                <p>Đơn hàng #{orderNumber} của bạn đã được cập nhật trạng thái.</p>
                <p><strong>Trạng thái mới:</strong> {status}</p>
                <p>Bạn có thể theo dõi chi tiết đơn hàng trong tài khoản của mình.</p>
                <p>Trân trọng,<br>Shop Technology Team</p>
            </body>
            </html>";

        return await SendEmailAsync(email, subject, body, true);
    }
}
