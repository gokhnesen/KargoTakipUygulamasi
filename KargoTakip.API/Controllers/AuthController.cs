using KargoTakip.Application.Features.Authentication.Commands.Login;
using KargoTakip.Application.Features.Authentication.Commands.Register;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KargoTakip.API.Controllers
{
    [Route("api/[controller]")]
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
    }
}