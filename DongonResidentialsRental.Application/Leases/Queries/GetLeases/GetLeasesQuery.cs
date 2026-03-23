using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Queries.GetLeases;

public sealed record GetLeasesQuery(
    string? SearchTerm,
    Guid? BuildingId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<LeaseResponse>>;
