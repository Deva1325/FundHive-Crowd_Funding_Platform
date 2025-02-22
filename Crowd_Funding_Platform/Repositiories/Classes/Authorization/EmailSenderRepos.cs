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

        public async Task SendEmailAsync(string toEmail, string subject, string body, string emailType)
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
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
                    .header {{ text-align: center; margin-bottom: 20px; }}
                    .content {{ font-size: 16px; line-height: 1.5; }}
                    .otp {{ font-size: 24px; font-weight: bold; color: #ff4500; text-align: center; margin: 20px 0; }}
                    .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Welcome to Bug Tracking System</h2>
                    </div>
                    <div class='content'>
                        <p>Dear Admin,</p>
                        <p>We are excited to have you as a part of our Bug Tracking System. As an admin, you will have access to manage and monitor the system effectively.</p>
                        <p>Your One-Time Password (OTP) for account verification is:</p>
                        <div class='otp'>{body}</div>
                        <p>Please enter this OTP on the verification page to complete your registration process.</p>
                        <p>If you did not request this registration, please ignore this email.</p>
                        <p>Thank you!</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} Bug Tracking System. All rights reserved.
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
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
                    .header {{ text-align: center; margin-bottom: 20px; }}
                    .content {{ font-size: 16px; line-height: 1.5; }}
                    .link {{ font-size: 18px; font-weight: bold; color: #007bff; text-align: center; margin: 20px 0; }}
                    .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Password Reset Request</h2>
                    </div>
                    <div class='content'>
                        <p>Dear Admin,</p>
                        <p>We received a request to reset the password for your Bug Tracking System account.</p>
                        <p>Click the link below to reset your password:</p>
                        <div class='otp'>
                            {body}
                        </div>
                        <p>If you did not request this, please ignore this email. Your account is safe, and no changes have been made.</p>
                        <p>Thank you!</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} Bug Tracking System. All rights reserved.
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
