using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnits;

public sealed record GetUnitsQuery(
    UnitStatus Status,
    string UnitNumber,
    BuildingId? BuildingId,
    int? Floor,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<UnitResponse>>;
