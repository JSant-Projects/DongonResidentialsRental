using FluentValidation;

namespace DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;

public sealed class CreateBuildingCommandValidator : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressStreet)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressCity)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressProvince)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressPostalCode)
            .NotEmpty()
            .MaximumLength(200);
    }
}
