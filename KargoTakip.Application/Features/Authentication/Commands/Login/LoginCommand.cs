using MediatR;

namespace KargoTakip.Application.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<LoginCommandResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}