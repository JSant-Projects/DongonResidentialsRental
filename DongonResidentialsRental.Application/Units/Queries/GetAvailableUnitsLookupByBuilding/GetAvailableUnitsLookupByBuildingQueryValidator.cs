using FluentValidation;

namespace DongonResidentialsRental.Application.Units.Queries.GetAvailableUnitsLookupByBuilding;

public sealed class GetAvailableUnitsLookupByBuildingQueryValidator : AbstractValidator<GetAvailableUnitsLookupByBuildingQuery>
{
    public GetAvailableUnitsLookupByBuildingQueryValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotNull()
            .Must(id => id is not null && id.Id != Guid.Empty)
            .WithMessage("BuildingId is required.");
    }
}
