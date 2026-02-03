using MediatR;

namespace KargoTakip.Application.Features.Authentication.Commands.ConfirmEmail
{
    public class ConfirmEmailCommand : IRequest<ConfirmEmailCommandResponse>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
