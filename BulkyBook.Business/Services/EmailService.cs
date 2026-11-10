using BulkyBook.Business.IServices;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
using Microsoft.Extensions.Configuration;


namespace BulkyBook.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["Mailjet:ApiKey"];
            _secretKey = _configuration["Mailjet:SecretKey"];
            _senderEmail = _configuration["Mailjet:SenderEmail"];
            _senderName = _configuration["Mailjet:SenderName"];
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var fromSendContact = new SendContact(_senderEmail, _senderEmail);
                var ToSendContact = new SendContact(toEmail);

                MailjetClient client = new MailjetClient(_apiKey, _secretKey);
                var email = new TransactionalEmailBuilder().
                    WithFrom(fromSendContact).WithTo(ToSendContact).WithSubject(subject).WithHtmlPart(htmlContent).Build();

                var response = await client.SendTransactionalEmailAsync(email);

                if (response.Messages != null && response.Messages.Length > 0)
                {
                    var message = response.Messages[0];
                    if (message.Status == "success")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(string toEmail, int orderId, decimal orderTotal)
        {
            var subject = $"Order Confirmation #{orderId} - BulkyBook";

            var htmlContent = $@"
                <h1>Thank you for your order!</h1>
                <p>Your order has been placed successfully.</p>
                <hr/>
                <p><strong>Order Number:</strong> {orderId}</p>
                <p><strong>Order Date:</strong> {DateTime.Now:MMM: dd, yyy}</p>
                <hr/>
                <p>Thank you for shopping with BulkyBook!</p>
                <p>- The BulkyBook Team</p>";

            return await SendEmailAsync(toEmail, subject, htmlContent);
        }
    }
}
