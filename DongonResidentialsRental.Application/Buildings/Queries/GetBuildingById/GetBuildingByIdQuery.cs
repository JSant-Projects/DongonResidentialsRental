using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildingById;

public sealed record GetBuildingByIdQuery(BuildingId BuildingId): IQuery<BuildingResponse>;
