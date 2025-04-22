using System.Net;
using System.Net.Mail;
using Crowd_Funding_Platform.Models;
using Microsoft.Extensions.Configuration;

namespace Crowd_Funding_Platform.Helpers
{
    public class EmailHelper
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailHelper(IConfiguration configuration)
        {
            _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
        }

        public void SendThankYouEmail(string toEmail, string userName, byte[] pdfBytes)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_smtpSettings.SenderEmail, "FundHive");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = "Thank You for Your Contribution!";
            message.Body = $"Dear {userName},\n\nThank you for your generous support on FundHive! Please find your receipt attached.\n\nRegards,\nFundHive Team";
            message.IsBodyHtml = false;

            // PDF attachment
            if (pdfBytes != null)
            {
                Attachment pdfAttachment = new Attachment(new MemoryStream(pdfBytes), "Receipt.pdf", "application/pdf");
                message.Attachments.Add(pdfAttachment);
            }

            var smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword),
                EnableSsl = _smtpSettings.EnableSsl
            };

            smtpClient.Send(message);
        }
    }
}