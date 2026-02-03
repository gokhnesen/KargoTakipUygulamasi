using MediatR;

namespace KargoTakip.Application.Features.Authentication.Commands.ResendConfirmationEmail
{
    public class ResendConfirmationEmailCommand : IRequest<ResendConfirmationEmailCommandResponse>
    {
        public string Email { get; set; }
    }
}
