//using Azure;
//using KargoTakip.Application.Interfaces.Notification;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading.Tasks;

//namespace KargoTakip.Infrastructure.Services
//{
//    public class AzureCommunicationEmailService : IEmailService
//    {
//        private readonly EmailClient _emailClient;
//        private readonly string _senderAddress;
//        private readonly ILogger<AzureCommunicationEmailService> _logger;

//        public AzureCommunicationEmailService(
//            IConfiguration configuration,
//            ILogger<AzureCommunicationEmailService> logger)
//        {
//            var connectionString = configuration["AzureCommunicationServices:ConnectionString"];
//            _senderAddress = configuration["AzureCommunicationServices:SenderEmail"];
//            _emailClient = new EmailClient(connectionString);
//            _logger = logger;
//        }

//        public async Task SendAsync(string toEmail, string subject, string body, string htmlBody = null)
//        {
//            try
//            {
//                var emailContent = new EmailContent(subject)
//                {
//                    PlainText = body,
//                    Html = htmlBody ?? body
//                };

//                var emailMessage = new EmailMessage(_senderAddress, toEmail, emailContent);

//                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
//                    WaitUntil.Completed,
//                    emailMessage);

//                _logger.LogInformation("Email gönderildi. Status: {Status}, OperationId: {OperationId}",
//                    emailSendOperation.Value.Status, emailSendOperation.Id);
//            }
//            catch (RequestFailedException ex)
//            {
//                _logger.LogError(ex, "Azure Communication Services email hatası. ErrorCode: {ErrorCode}",
//                    ex.ErrorCode);
//                throw;
//            }
//        }
//    }
//}