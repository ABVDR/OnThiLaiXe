namespace OnThiLaiXe.Services
{
    public interface IGmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);

    }
}
