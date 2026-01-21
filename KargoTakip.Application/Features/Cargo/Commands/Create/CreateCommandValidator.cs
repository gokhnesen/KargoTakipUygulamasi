using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.Cargo.Commands.Create
{
    public class CreateCommandValidator : AbstractValidator<CreateCargoCommand>
    {
        public CreateCommandValidator()
        {
            RuleFor(x => x.SenderFullName)
                .NotEmpty().WithMessage("Gönderen kişinin adı boş bırakılamaz.")
                .Length(3, 100).WithMessage("Gönderen kişinin adı 3 ile 100 karakter arasında olmalıdır.");

            RuleFor(x => x.SenderAddress)
                .NotEmpty().WithMessage("Gönderen kişinin adresi boş bırakılamaz.")
                .MinimumLength(10).WithMessage("Gönderen kişinin adresi en az 10 karakter olmalıdır.")
                .MaximumLength(300).WithMessage("Gönderen kişinin adresi 300 karakteri geçemez.");

            RuleFor(x => x.SenderPhoneNumber)
                .NotEmpty().WithMessage("Gönderen kişinin telefon numarası boş bırakılamaz.")
                .Matches(@"^[0-9]{10,15}$").WithMessage("Telefon numarası 10-15 rakam arasında olmalıdır.");

            RuleFor(x => x.ReceiverName)
                .NotEmpty().WithMessage("Alıcı kişinin adı boş bırakılamaz.")
                .Length(2, 50).WithMessage("Alıcı kişinin adı 2 ile 50 karakter arasında olmalıdır.");

            RuleFor(x => x.ReceiverSurname)
                .NotEmpty().WithMessage("Alıcı kişinin soyadı boş bırakılamaz.")
                .Length(2, 50).WithMessage("Alıcı kişinin soyadı 2 ile 50 karakter arasında olmalıdır.");

            RuleFor(x => x.ReceiverAddress)
                .NotEmpty().WithMessage("Alıcı kişinin adresi boş bırakılamaz.")
                .MinimumLength(10).WithMessage("Alıcı kişinin adresi en az 10 karakter olmalıdır.")
                .MaximumLength(300).WithMessage("Alıcı kişinin adresi 300 karakteri geçemez.");

            RuleFor(x => x.ReceiverPhoneNumber)
                .NotEmpty().WithMessage("Alıcı kişinin telefon numarası boş bırakılamaz.")
                .Matches(@"^[0-9]{10,15}$").WithMessage("Telefon numarası 10-15 rakam arasında olmalıdır.");

            RuleFor(x => x.ReceiverEmail)
                .NotEmpty().WithMessage("Alıcı kişinin e-posta adresi boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(100).WithMessage("E-posta adresi 100 karakteri geçemez.");
        }
    }
}
