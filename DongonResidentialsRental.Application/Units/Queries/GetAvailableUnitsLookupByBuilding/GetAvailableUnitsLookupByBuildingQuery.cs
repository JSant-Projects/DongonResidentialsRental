using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetAvailableUnitsLookupByBuilding;

public sealed record GetAvailableUnitsLookupByBuildingQuery(BuildingId BuildingId) : IQuery<IReadOnlyList<UnitLookupResponse>>;
