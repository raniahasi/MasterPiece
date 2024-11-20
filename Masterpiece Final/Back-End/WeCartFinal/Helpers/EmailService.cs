using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
namespace Wecartcore.DTO
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string message)
        {
            // Load email settings from appsettings.json
            var emailSettings = _configuration.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("WeCart", senderEmail));
            email.To.Add(new MailboxAddress("Admin", recipientEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = message };

            using (var smtpClient = new SmtpClient())
            {
                // Connect to the SMTP server
                smtpClient.Connect(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);

                // Authenticate using the sender's email and password
                smtpClient.Authenticate(senderEmail, senderPassword);

                // Send the email
                await smtpClient.SendAsync(email);

                // Disconnect from the SMTP server
                smtpClient.Disconnect(true);
            }
        }
    }
}
