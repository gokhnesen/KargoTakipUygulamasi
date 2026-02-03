namespace KargoTakip.Application.Features.Authentication.Commands.VerifyLoginOtp
{
    public class VerifyLoginOtpCommandResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public string Message { get; set; }
    }
}
