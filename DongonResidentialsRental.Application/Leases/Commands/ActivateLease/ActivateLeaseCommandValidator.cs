using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.ActivateLease;

public class ActivateLeaseCommandValidator : AbstractValidator<ActivateLeaseCommand>
{
    public ActivateLeaseCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();
    }
}
