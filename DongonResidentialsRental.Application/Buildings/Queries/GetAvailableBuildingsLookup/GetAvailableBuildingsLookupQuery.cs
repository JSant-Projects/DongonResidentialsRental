using DongonResidentialsRental.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetAvailableBuildingsLookup;

public sealed record GetAvailableBuildingsLookupQuery() : IQuery<IReadOnlyList<BuildingLookupResponse>>;
