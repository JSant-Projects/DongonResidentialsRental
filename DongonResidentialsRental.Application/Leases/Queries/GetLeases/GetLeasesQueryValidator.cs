using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Queries.GetLeases;

public sealed class GetLeasesQueryValidator : AbstractValidator<GetLeasesQuery>
{
    public GetLeasesQueryValidator()
    {
        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
