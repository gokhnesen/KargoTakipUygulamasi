namespace KargoTakip.Application.Messages
{
    /// <summary>
    /// Message payload for queuing email send operations via RabbitMQ.
    /// </summary>
    public class SendEmailMessage
    {
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string PlainBody { get; set; } = string.Empty;
        public string? HtmlBody { get; set; }
        /// <summary>
        /// Optional type for logging/metrics: KayitOnay, LoginOtp, ResendOnay, KargoDurumGuncelleme.
        /// </summary>
        public string? MessageType { get; set; }
    }
}
