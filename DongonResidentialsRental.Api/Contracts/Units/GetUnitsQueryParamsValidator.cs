using DongonResidentialsRental.Application.Extensions;
using FluentValidation;

namespace DongonResidentialsRental.Api.Contracts.Units;

public class GetUnitsQueryParamsValidator : AbstractValidator<GetUnitsQueryParams>
{
    public GetUnitsQueryParamsValidator()
    {
        RuleFor(x => x.Status)
            .MustBeEnumValue<GetUnitsQueryParams, Domain.Unit.UnitStatus>(allowEmpty: true);
    }
}
