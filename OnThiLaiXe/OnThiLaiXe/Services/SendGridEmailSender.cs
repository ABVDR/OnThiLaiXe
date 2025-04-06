using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OnThiLaiXe.Services
{
    public class SendGridEmailSender : IGmailSender
    {
        private readonly string _apiKey;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(IConfiguration configuration, ILogger<SendGridEmailSender> logger)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("Lỗi SendGrid API Key ");
                throw new Exception(" Lỗi SendGrid API Key");
            }

            var fromEmail = "ngocchau0845@gmail.com"; // Phải là email đã xác thực trên SendGrid
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(fromEmail, "Your App Name");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
            msg.SetClickTracking(false, false);

            _logger.LogInformation($" Đang gửi email đến : {toEmail}...");
            var response = await client.SendEmailAsync(msg);
            var responseBody = await response.Body.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation($"Đã gửi email đến: {toEmail}");
            }
            else
            {
                _logger.LogError($"Gửi email thất bại: {response.StatusCode}");
                _logger.LogError($"Nội dung phản hồi từ SendGrid: {responseBody}");
                throw new Exception($"Gửi email thất bại. Phản hồi từ SendGrid: {responseBody}");
            }
        }
    }
}
