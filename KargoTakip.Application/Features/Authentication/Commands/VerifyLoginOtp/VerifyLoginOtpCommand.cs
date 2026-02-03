using MediatR;

namespace KargoTakip.Application.Features.Authentication.Commands.VerifyLoginOtp
{
    public class VerifyLoginOtpCommand : IRequest<VerifyLoginOtpCommandResponse>
    {
        public string LoginToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
