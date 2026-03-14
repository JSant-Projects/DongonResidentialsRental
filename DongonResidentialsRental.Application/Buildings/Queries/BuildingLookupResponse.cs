using DongonResidentialsRental.Domain.Building;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries;

public sealed record BuildingLookupResponse(Guid BuildingId, string Name);
