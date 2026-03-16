using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.CreateLease;

public sealed class CreateLeaseCommandValidator : AbstractValidator<CreateLeaseCommand>
{
    public CreateLeaseCommandValidator()
    {
        RuleFor(x => x.Occupancy.Id)
            .NotNull();

        RuleFor(x => x.UnitId.Id)
            .NotNull();

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.MonthlyRate)
            .NotNull()
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);

    }
}








