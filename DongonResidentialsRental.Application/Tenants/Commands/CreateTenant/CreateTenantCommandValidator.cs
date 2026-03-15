using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotNull()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotNull()
            .MaximumLength(254);

        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .MaximumLength(20);
    }
}
