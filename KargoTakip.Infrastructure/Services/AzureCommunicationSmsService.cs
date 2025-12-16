//using Azure;
//using Azure.Communication.Sms;
//using KargoTakip.Application.Interfaces.Notification;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading.Tasks;

//namespace KargoTakip.Infrastructure.Services
//{
//    public class AzureCommunicationSmsService : ISmsService
//    {
//        private readonly SmsClient _smsClient;
//        private readonly string _senderPhoneNumber;
//        private readonly ILogger<AzureCommunicationSmsService> _logger;

//        public AzureCommunicationSmsService(
//            IConfiguration configuration,
//            ILogger<AzureCommunicationSmsService> logger)
//        {
//            var connectionString = configuration["AzureCommunicationServices:ConnectionString"];
//            _senderPhoneNumber = configuration["AzureCommunicationServices:SenderPhoneNumber"];
//            _smsClient = new SmsClient(connectionString);
//            _logger = logger;
//        }

//        public async Task SendAsync(string phoneNumber, string message)
//        {
//            try
//            {
//                SmsSendResult sendResult = await _smsClient.SendAsync(
//                    from: _senderPhoneNumber,
//                    to: phoneNumber,
//                    message: message,
//                    options: new SmsSendOptions(enableDeliveryReport: true)
//                    {
//                        Tag = "cargo-notification"
//                    });

//                _logger.LogInformation("SMS gönderildi. MessageId: {MessageId}, Success: {Success}",
//                    sendResult.MessageId, sendResult.Successful);

//                if (!sendResult.Successful)
//                {
//                    _logger.LogError("SMS gönderilemedi. ErrorMessage: {ErrorMessage}",
//                        sendResult.ErrorMessage);
//                }
//            }
//            catch (RequestFailedException ex)
//            {
//                _logger.LogError(ex, "Azure Communication Services SMS hatası. ErrorCode: {ErrorCode}",
//                    ex.ErrorCode);
//                throw;
//            }
//        }
//    }
//}