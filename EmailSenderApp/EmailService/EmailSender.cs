using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EmailSenderApp.EmailService
{
    public class EmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;
        private readonly int _delayMs;

        public EmailSender(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromEmail, int delayMs = 5000)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
            _fromEmail = fromEmail;
            _delayMs = delayMs;
        }

        public async Task SendEmailsAsync(List<string> recipients, string subject, string htmlBody, Action<string> logCallback)
        {
            foreach (var recipient in recipients)
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(MailboxAddress.Parse(_fromEmail));
                    message.To.Add(MailboxAddress.Parse(recipient));
                    message.Subject = subject;

                    var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                    message.Body = bodyBuilder.ToMessageBody();

                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);

                    logCallback?.Invoke($"Sent to {recipient}");
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"Failed to {recipient}: {ex.Message}");
                }

                await Task.Delay(_delayMs);
            }
        }
    }
}
