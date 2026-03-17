using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;

public sealed class GetBuildingQueryValidator : AbstractValidator<GetBuildingsQuery>
{
    public GetBuildingQueryValidator()
    {
        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
