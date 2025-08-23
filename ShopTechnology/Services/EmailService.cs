using System.Net.Mail;
using System.Net;

namespace ShopTechnology.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Chào mừng bạn đến với Shop Technology Accessories";
            var body = $@"
                <h2>Xin chào {fullName}!</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại Shop Technology Accessories.</p>
                <p>Chúng tôi rất vui mừng được phục vụ bạn!</p>
                <p>Trân trọng,<br/>Shop Technology Accessories Team</p>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink)
        {
            var subject = "Đặt lại mật khẩu - Shop Technology Accessories";
            var body = $@"
                <h2>Xin chào {fullName}!</h2>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                <p>Vui lòng click vào liên kết bên dưới để đặt lại mật khẩu:</p>
                <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
                <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                <p>Trân trọng,<br/>Shop Technology Accessories Team</p>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendOrderConfirmationEmailAsync(string email, string fullName, string orderNumber)
        {
            var subject = $"Xác nhận đơn hàng #{orderNumber} - Shop Technology Accessories";
            var body = $@"
                <h2>Xin chào {fullName}!</h2>
                <p>Cảm ơn bạn đã đặt hàng tại Shop Technology Accessories.</p>
                <p>Mã đơn hàng của bạn là: <strong>{orderNumber}</strong></p>
                <p>Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.</p>
                <p>Bạn sẽ nhận được email cập nhật khi đơn hàng được xử lý.</p>
                <p>Trân trọng,<br/>Shop Technology Accessories Team</p>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendOrderStatusUpdateEmailAsync(string email, string fullName, string orderNumber, string status)
        {
            var subject = $"Cập nhật trạng thái đơn hàng #{orderNumber} - Shop Technology Accessories";
            var body = $@"
                <h2>Xin chào {fullName}!</h2>
                <p>Đơn hàng #{orderNumber} của bạn đã được cập nhật trạng thái.</p>
                <p>Trạng thái mới: <strong>{status}</strong></p>
                <p>Bạn có thể theo dõi đơn hàng của mình trong tài khoản cá nhân.</p>
                <p>Trân trọng,<br/>Shop Technology Accessories Team</p>";

            await SendEmailAsync(email, subject, body, true);
        }

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                // In a real application, you would configure SMTP settings in appsettings.json
                // For now, we'll just log the email (you can implement actual email sending later)
                Console.WriteLine($"Email would be sent to: {to}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                
                // Example SMTP configuration (uncomment and configure for actual email sending):
                /*
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                */
            }
            catch (Exception ex)
            {
                // Log the error (in a real application, use proper logging)
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
