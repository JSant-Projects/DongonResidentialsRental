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
            .NotEmpty();

        RuleFor(x => x.UnitId.Id)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly));

        RuleFor(x => x.MonthlyRate)
            .NotNull()
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(x => x.DueDayOfMonth)
            .NotNull()
            .GreaterThan(0);

        RuleFor(x => x.GracePeridoDays)
            .NotNull()
            .GreaterThan(0);

    }
}








