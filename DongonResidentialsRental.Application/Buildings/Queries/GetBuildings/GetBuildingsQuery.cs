using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Building;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;

public sealed record GetBuildingsQuery(
    BuildingStatus? Status, 
    string? SearchTerm, 
    int Page = 1, 
    int PageSize = 20) : IQuery<PagedResult<BuildingResponse>>;
