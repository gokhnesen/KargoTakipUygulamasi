using FluentValidation;
using KargoTakip.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Application.Features.CargoMovement.Commands.UpdateCargoMovement
{
    public class UpdateCargoMovementCommandValidator : AbstractValidator<UpdateCargoMovementCommand>
    {
        public UpdateCargoMovementCommandValidator()
        {
            RuleFor(x => x.CargoId)
                .NotEmpty().WithMessage("Kargo ID bo? b?rak?lamaz.")
                .Length(36).WithMessage("Geçerli bir Kargo ID giriniz.");

            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Geçerli bir kargo durumu seçiniz.")
                .NotEqual(CargoStatus.Received).WithMessage("Kargo durumu 'Received' olarak güncellenemez.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Lokasyon bilgisi bo? b?rak?lamaz.")
                .Length(3, 200).WithMessage("Lokasyon bilgisi 3 ile 200 karakter aras?nda olmal?d?r.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Aç?klama 500 karakteri geçemez.");
        }
    }
}