using KargoTakip.Application.Features.Authentication.Commands.ConfirmEmail;
using KargoTakip.Application.Features.Authentication.Commands.Login;
using KargoTakip.Application.Features.Authentication.Commands.Register;
using KargoTakip.Application.Features.Authentication.Commands.ResendConfirmationEmail;
using KargoTakip.Application.Features.Authentication.Commands.VerifyLoginOtp;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KargoTakip.API.Controllers
{
    public class AuthController : BaseController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyLoginOtpCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}