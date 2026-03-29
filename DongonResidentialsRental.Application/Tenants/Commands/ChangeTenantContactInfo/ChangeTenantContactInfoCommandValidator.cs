using FluentValidation;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantContactInfo;

public sealed class ChangeTenantContactInfoCommandValidator : AbstractValidator<ChangeTenantContactInfoCommand>
{
    public ChangeTenantContactInfoCommandValidator()
    {
        RuleFor(x => x.TenantId.Id)
            .NotEmpty();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .NotNull()
            .MaximumLength(254);
    }
}