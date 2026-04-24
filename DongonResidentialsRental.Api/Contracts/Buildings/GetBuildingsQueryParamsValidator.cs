using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Domain.Building;
using FluentValidation;

namespace DongonResidentialsRental.Api.Contracts.Buildings;

public class GetBuildingsQueryParamsValidator : AbstractValidator<GetBuildingsQueryParams>
{
    public GetBuildingsQueryParamsValidator()
    {
        RuleFor(x => x.Status)
            .MustBeEnumValue<GetBuildingsQueryParams, BuildingStatus>(allowEmpty: true);
    }
}
