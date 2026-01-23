using KargoTakip.Application.Interfaces.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Services
{
    public class InfobipSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InfobipSmsService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _sender;

        public InfobipSmsService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InfobipSmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _baseUrl = _configuration["Infobip:BaseUrl"] ?? "https://api.infobip.com";
            _apiKey = _configuration["Infobip:ApiKey"];
            _sender = _configuration["Infobip:Sender"] ?? "KargoTakip";

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Infobip API Key yap?land?r?lmam??!");
            }
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Infobip API Key bulunamad?. SMS gönderilemiyor.");
                return;
            }

            try
            {
                // Telefon numaras?n? formatla (Türkiye için +90 ekle)
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                // Sender bo? b?rak?l?rsa Infobip default sender kullan?r
                var messageBody = new
                {
                    destinations = new[]
                    {
                        new { to = formattedNumber }
                    },
                    text = message
                };

                // E?er sender ayarlanm??sa ekle, yoksa default kullan
                var requestBody = string.IsNullOrEmpty(_sender) 
                    ? new { messages = new[] { messageBody } }
                    : new
                    {
                        messages = new[]
                        {
                            new
                            {
                                destinations = new[]
                                {
                                    new { to = formattedNumber }
                                },
                                from = _sender,
                                text = message
                            }
                        }
                    };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"App {_apiKey}");

                var response = await _httpClient.PostAsync($"{_baseUrl}/sms/2/text/advanced", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("SMS ba?ar?yla gönderildi. Telefon: {Phone}, Response: {Response}", 
                        formattedNumber, responseContent);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("SMS gönderilemedi. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"Infobip SMS gönderimi ba?ar?s?z: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS gönderme s?ras?nda hata olu?tu. Telefon: {Phone}", phoneNumber);
                throw;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Bo?luklar? ve özel karakterleri temizle
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // E?er + ile ba?lam?yorsa ve 0 ile ba?l?yorsa, 0'? kald?r ve +90 ekle
            if (!cleaned.StartsWith("+"))
            {
                if (cleaned.StartsWith("0"))
                {
                    cleaned = cleaned.Substring(1);
                }
                cleaned = "+90" + cleaned;
            }

            return cleaned;
        }
    }
}
