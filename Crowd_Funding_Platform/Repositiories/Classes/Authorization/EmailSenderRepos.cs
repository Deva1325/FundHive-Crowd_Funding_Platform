using Crowd_Funding_Platform.Models;
using Crowd_Funding_Platform.Repositiories.Interfaces.IAuthorization;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using static System.Net.WebRequestMethods;
using System.Text.RegularExpressions;

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

        public async Task SendEmailAsync(string toEmail, string userName, string subject, string body, string emailType)
        {
            try
            {
                string certificateFilePath = null;
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("OTP Verifier", "tester.devanshi29@gmail.com"));
                message.To.Add(new MailboxAddress("User", toEmail));
                message.Subject = subject;

                string emailBody = string.Empty;

                if (emailType == "Registration")
                {
                    emailBody = $@"<html>
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
                                &copy; {DateTime.Now.Year} FundHive. All rights reserved.
                            </div>
                        </div>
                    </body>
                </html>";
                }
                else if (emailType == "ForgotPassword")
                {
                    emailBody = $@"<html>
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
                                <p>We received a request to reset your password for your account on FundHive.</p>
                                <p>To reset your password, please click the link below:</p>
                                {body}
                                <p>If you did not request this, please ignore this email. Your account is safe.</p>
                                <p>Need help? Feel free to contact our support team.</p>
                            </div>
                            <div class='footer'>
                                &copy; {DateTime.Now.Year} FundHive. All rights reserved.
                            </div>
                        </div>
                    </body>
                </html>";
                }
                else if (emailType == "Contribution")
                {
                    emailBody = $@"<html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; margin: 0; }}
                            .container {{ max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
                            .header {{ text-align: center; margin-bottom: 20px; }}
                            .header h2 {{ color: #333; }}
                            .content {{ font-size: 16px; line-height: 1.6; color: #555; text-align: left; }}
                            .highlight {{ font-weight: bold; color: #28a745; }}
                            .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Thank You for Your Contribution!</h2>
                            </div>
                            <div class='content'>
                                <p>Dear {userName},</p>
                                <p>We have successfully received your contribution. Thank you for supporting this campaign and making an impact!</p>
                                <p><span class='highlight'>Receipt Details:</span></p>
                                {body}
                               
                            </div>
                            <div class='footer'>
                                &copy; {DateTime.Now.Year} FundHive. All rights reserved.
                            </div>
                        </div>
                    </body>
                </html>";
                }
                else if (emailType == "Certificate")
                {
                    string badgeName = "Gold Badge";

                    // Extract badge name and certificate path from the input
                    var match = Regex.Match(body, @"^(.*?)\|CERTIFICATE_PATH:(.+)$");
                    if (match.Success)
                    {
                        badgeName = match.Groups[1].Value.Trim();
                        certificateFilePath = match.Groups[2].Value.Trim();
                    }

                    emailBody = $@"<html>
<head>
    <style>
        body {{
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
            font-family: Arial, Helvetica, sans-serif;
        }}
        .email-container {{
            background: #ffffff;
            max-width: 600px;
            margin: 40px auto;
            padding: 30px;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
            text-align: center;
        }}
        h2 {{
            color: #4CAF50;
            font-size: 24px;
            margin-bottom: 20px;
        }}
        p {{
            color: #333333;
            font-size: 15px;
            line-height: 1.6;
            margin: 10px 0;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #777777;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <h2>🎉 Congratulations on Earning Your Badge!</h2>
        <p>Dear {userName},</p>
        <p>We are delighted to present you with the <strong>{badgeName}</strong> for your outstanding contributions at <strong>FundHive</strong>.</p>
        <p>This badge is a token of your dedication and commitment to supporting meaningful campaigns.</p>
        <p>Please find your official certificate attached as a PDF.</p>
        <p>Thank you for being an invaluable part of the FundHive community!</p>
        <div class='footer'>
            &mdash; The FundHive Team
        </div>
    </div>
</body>
</html>";
                }
                else if (emailType == "GoogleRegistration")
                {
                    string username = userName;
                    string email = "";
                    string password = "";

                    // Parse the body using a delimiter
                    var match = Regex.Match(body, @"^(.*?)\|EMAIL:(.*?)\|PASSWORD:(.*)$");
                    if (match.Success)
                    {
                        username = match.Groups[1].Value.Trim();
                        email = match.Groups[2].Value.Trim();
                        password = match.Groups[3].Value.Trim();
                    }

                    emailBody = $@"<html>
<head>
    <style>
        body {{
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.05);
        }}
        h2 {{
            color: #4CAF50;
            text-align: center;
        }}
        p {{
            font-size: 16px;
            color: #333333;
            line-height: 1.6;
        }}
        .info {{
            background-color: #f1f1f1;
            padding: 15px;
            border-radius: 6px;
            margin: 20px 0;
        }}
        .info strong {{
            display: inline-block;
            width: 100px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #999;
            margin-top: 30px;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <h2>🎉 Welcome to FundHive!</h2>
        <p>Hi {username},</p>
        <p>Your account has been successfully created using your Google account.</p>

        <div class='info'>
            <p><strong>Username:</strong> {username}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Password:</strong> {password}</p>
        </div>

        <p>You can now log in and start exploring campaigns or even create your own.</p>
        <p><strong>Tip:</strong> For better security, we recommend updating your password after your first login.</p>

        <p>If you have any questions or need help, feel free to contact our support team.</p>

        <div class='footer'>
            &mdash; The FundHive Team
        </div>
    </div>
</body>
</html>";
                }

                var builder = new BodyBuilder();
                builder.HtmlBody = emailBody;

                //// Optional: attach PDF if it's a contribution email
                if (emailType == "Contribution")
                {
                    var match = Regex.Match(body, @"<!--\s*PDF-ATTACHMENT:(.+?)\s*-->", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        string base64Pdf = match.Groups[1].Value.Trim();
                        byte[] pdfBytes = Convert.FromBase64String(base64Pdf);
                        builder.Attachments.Add("FundHive_Receipt.pdf", pdfBytes, new ContentType("application", "pdf"));

                        // Optionally remove the placeholder from the email body
                        body = Regex.Replace(body, @"<!--\s*PDF-ATTACHMENT:(.+?)\s*-->", "");
                    }
                }
                // Attach certificate if the file exists
                if (!string.IsNullOrEmpty(certificateFilePath) && System.IO.File.Exists(certificateFilePath))
                {
                    builder.Attachments.Add(Path.GetFileName(certificateFilePath), System.IO.File.ReadAllBytes(certificateFilePath), new ContentType("application", "pdf"));
                }

                message.Body = builder.ToMessageBody();

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

        //public async Task SendEmailAsync(string toEmail,string userName, string subject, string body, string emailType)
        //{
        //    try
        //    {
        //        var message = new MimeMessage();
        //        message.From.Add(new MailboxAddress("OTP Verifier", "tester.devanshi29@gmail.com"));
        //        message.To.Add(new MailboxAddress("User", toEmail));
        //        message.Subject = subject;

        //        // Email content based on the email type
        //        string emailBody = string.Empty;

        //    if (emailType == "Registration")
        //    {
        //        emailBody = $@"
        //    <html>
        //        <head>
        //            <style>
        //                body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; margin: 0; }}
        //                .container {{ max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
        //                .header {{ text-align: center; margin-bottom: 20px; }}
        //                .header h2 {{ color: #333; }}
        //                .content {{ font-size: 16px; line-height: 1.6; color: #555; text-align: left; }}
        //                .otp {{ font-size: 22px; font-weight: bold; color: #ff4500; text-align: center; margin: 20px 0; padding: 10px; background: #fff3e6; border-radius: 5px; }}
        //                .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
        //            </style>
        //        </head>
        //        <body>
        //            <div class='container'>
        //                <div class='header'>
        //                    <h2>Welcome to FundHive!</h2>
        //                </div>
        //                <div class='content'>
        //                    <p>Dear {userName},</p>
        //                    <p>Thank you for registering on our crowdfunding platform. We are excited to have you onboard!</p>
        //                    <p>To verify your account, please use the following One-Time Password (OTP):</p>
        //                    <div class='otp'>{body}</div>
        //                    <p>Please enter this OTP on the verification page to complete your registration.</p>
        //                    <p>If you did not request this, you can safely ignore this email.</p>
        //                    <p>We look forward to seeing you create and support impactful campaigns!</p>
        //                </div>
        //                <div class='footer'>
        //                    &copy; {DateTime.Now.Year} [Your Crowdfunding Platform Name]. All rights reserved.
        //                </div>
        //            </div>
        //        </body>
        //    </html>";
        //    }
        //    else if (emailType == "ForgotPassword")
        //    {
        //        emailBody = $@"
        //    <html>
        //        <head>
        //            <style>
        //                body {{ font-family: Arial, sans-serif; background-color: #f4f4f9; padding: 20px; margin: 0; }}
        //                .container {{ max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }}
        //                .header {{ text-align: center; margin-bottom: 20px; }}
        //                .header h2 {{ color: #333; }}
        //                .content {{ font-size: 16px; line-height: 1.6; color: #555; text-align: left; }}
        //                .reset-link {{ display: block; text-align: center; font-size: 18px; font-weight: bold; color: #007bff; margin: 20px 0; text-decoration: none; background: #e6f2ff; padding: 10px; border-radius: 5px; }}
        //                .footer {{ text-align: center; font-size: 12px; color: #888; margin-top: 20px; }}
        //            </style>
        //        </head>
        //        <body>
        //            <div class='container'>
        //                <div class='header'>
        //                    <h2>Password Reset Request</h2>
        //                </div>
        //                <div class='content'>
        //                    <p>Dear {userName},</p>
        //                    <p>We received a request to reset your password for your account on [Your Crowdfunding Platform Name].</p>
        //                    <p>To reset your password, please click the link below:</p>
        //                    {body}
        //                    <p>If you did not request this, please ignore this email. Your account is safe.</p>
        //                    <p>Need help? Feel free to contact our support team.</p>
        //                </div>
        //                <div class='footer'>
        //                    &copy; {DateTime.Now.Year} [Your Crowdfunding Platform Name]. All rights reserved.
        //                </div>
        //            </div>
        //        </body>
        //     </html>";
        //    }


        //        message.Body = new TextPart("html") { Text = emailBody };



        //        using (var smtpClient = new SmtpClient())
        //        {
        //            await smtpClient.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
        //            await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);
        //            await smtpClient.SendAsync(message);
        //            await smtpClient.DisconnectAsync(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //        throw;
        //    }

        //}
    }
}
