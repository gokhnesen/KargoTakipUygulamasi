using FluentValidation;
using KargoTakip.Domain.Enums;

namespace KargoTakip.Application.Features.CargoMovement.Commands.Create
{
    public class CreateCargoMovementCommandValidator : AbstractValidator<CreateCargoMovementCommand>
    {
        public CreateCargoMovementCommandValidator()
        {
            RuleFor(x => x.CargoId)
                .NotEmpty().WithMessage("Kargo ID boş olamaz")
                .NotNull().WithMessage("Kargo ID null olamaz");

            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Geçersiz kargo durumu");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Konum boş olamaz")
                .MaximumLength(200).WithMessage("Konum en fazla 200 karakter olabilir");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");
        }
    }
}