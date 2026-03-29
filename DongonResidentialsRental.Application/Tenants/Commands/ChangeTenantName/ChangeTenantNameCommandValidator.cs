using FluentValidation;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantName;

public sealed class ChangeTenantNameCommandValidator : AbstractValidator<ChangeTenantNameCommand>
{
    public ChangeTenantNameCommandValidator()
    {
        RuleFor(x => x.TenantId.Id)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
    }
}