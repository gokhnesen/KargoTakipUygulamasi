using FluentValidation;

namespace KargoTakip.Application.Features.Authentication.Commands.VerifyLoginOtp
{
    public class VerifyLoginOtpCommandValidator : AbstractValidator<VerifyLoginOtpCommand>
    {
        public VerifyLoginOtpCommandValidator()
        {
            RuleFor(x => x.LoginToken).NotEmpty().WithMessage("Giris token gerekli.");
            RuleFor(x => x.Code).NotEmpty().WithMessage("Dogrulama kodu gerekli.");
        }
    }
}
