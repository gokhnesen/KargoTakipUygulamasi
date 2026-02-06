namespace KargoTakip.Application.Messages
{
    public class SendEmailMessage
    {
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string PlainBody { get; set; } = string.Empty;
        public string? HtmlBody { get; set; }
        public string? MessageType { get; set; }
    }
}
