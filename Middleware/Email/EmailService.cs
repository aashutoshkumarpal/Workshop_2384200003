using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Email
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.To.Add(toEmail);
                    mail.From = new MailAddress(_configuration["EmailSettings:SenderEmail"]);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"])))
                    {
                        smtp.Credentials = new NetworkCredential(
                            _configuration["EmailSettings:SenderEmail"],
                            _configuration["EmailSettings:SenderPassword"]
                        );
                        smtp.EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]);
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Error: {ex.Message}");
            }
        }
    }
}
