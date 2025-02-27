using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using static System.Net.WebRequestMethods;

namespace Crowd_Funding_Platform.Repositiories.Classes.Authorization
{
    public class EmailSenderRepos : IEmailSenderRepos
    {

        private readonly SmtpSettings _smtpSettings;

        public EmailSenderRepos(IOptions<SmtpSettings> smtpSttings)
        {
            _smtpSettings = smtpSttings.Value;
        }

        public string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task SendEmailAsync(string toEmail,string userName, string subject, string body, string emailType)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("OTP Verifier", "tester.devanshi29@gmail.com"));
                message.To.Add(new MailboxAddress("User", toEmail));
                message.Subject = subject;

                // Email content based on the email type
                string emailBody = string.Empty;

            if (emailType == "Registration")
            {
                emailBody = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; margin: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
                        .header {{ text-align: center; margin-bottom: 20px; }}
                        .header h2 {{ color: #333; }}
                        .content {{ font-size: 16px; line-height: 1.6; color: #555; text-align: left; }}
                        .otp {{ font-size: 22px; font-weight: bold; color: #ff4500; text-align: center; margin: 20px 0; padding: 10px; background: #fff3e6; border-radius: 5px; }}
                        .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Welcome to FundHive!</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>Thank you for registering on our crowdfunding platform. We are excited to have you onboard!</p>
                            <p>To verify your account, please use the following One-Time Password (OTP):</p>
                            <div class='otp'>{body}</div>
                            <p>Please enter this OTP on the verification page to complete your registration.</p>
                            <p>If you did not request this, you can safely ignore this email.</p>
                            <p>We look forward to seeing you create and support impactful campaigns!</p>
                        </div>
                        <div class='footer'>
                            &copy; {DateTime.Now.Year} [Your Crowdfunding Platform Name]. All rights reserved.
                        </div>
                    </div>
                </body>
            </html>";
            }
            else if (emailType == "ForgotPassword")
            {
                emailBody = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; margin: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
                        .header {{ text-align: center; margin-bottom: 20px; }}
                        .header h2 {{ color: #333; }}
                        .content {{ font-size: 16px; line-height: 1.6; color: #555; text-align: left; }}
                        .reset-link {{ display: block; text-align: center; font-size: 18px; font-weight: bold; color: #007bff; margin: 20px 0; text-decoration: none; background: #e6f2ff; padding: 10px; border-radius: 5px; }}
                        .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Password Reset Request</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {userName},</p>
                            <p>We received a request to reset your password for your account on [Your Crowdfunding Platform Name].</p>
                            <p>To reset your password, please click the link below:</p>
                            {body}
                            <p>If you did not request this, please ignore this email. Your account is safe.</p>
                            <p>Need help? Feel free to contact our support team.</p>
                        </div>
                        <div class='footer'>
                            &copy; {DateTime.Now.Year} [Your Crowdfunding Platform Name]. All rights reserved.
                        </div>
                    </div>
                </body>
             </html>";
            }


                message.Body = new TextPart("html") { Text = emailBody };



                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }
    }
}
