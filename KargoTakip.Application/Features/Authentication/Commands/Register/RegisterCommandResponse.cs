namespace KargoTakip.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
    }
}