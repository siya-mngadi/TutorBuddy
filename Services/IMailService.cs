using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TutorBuddy.Services
{
    public interface IMailService
    {
        Task<Response> SendEmailAsync(string toEmail, string subject, string content);
    }

    public class SendGridMailService : IMailService
    {
        private IConfiguration _configuration;

        public SendGridMailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Response> SendEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            EmailAddress from;
            if (toEmail.Contains("mandela.ac.za"))
            {
                from = new EmailAddress("donotreplyplz@outlook.com", "TutorBuddy");
            }
            else
            {
                from = new EmailAddress("keizyjack5@gmail.com", "TutorBuddy");
            }
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to,subject,content,content);
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
}
