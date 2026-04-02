using CS58.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CS58.Services // Thay thế bằng namespace của bạn
{
    public class SendMailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<SendMailService> _logger;

        // Inject IOptions để lấy cấu hình từ appsettings.json và ILogger để ghi log
        public SendMailService(IOptions<MailSettings> mailSettings, ILogger<SendMailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            // Xây dựng nội dung email (hỗ trợ HTML)
            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                // Kết nối tới SMTP server
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                
                // Xác thực tài khoản
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                
                // Gửi email
                await smtp.SendAsync(message);
                
                // Ngắt kết nối
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation("Gửi email thành công tới: " + email);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi gửi email: " + ex.Message);

                // Tạo thư mục lưu email nếu gửi lỗi
                Directory.CreateDirectory("mailssave");
                var emailSaveFile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
                await message.WriteToAsync(emailSaveFile);
                
                _logger.LogInformation("Đã lưu email lỗi vào: " + emailSaveFile);
                
            }
        }
    }
}