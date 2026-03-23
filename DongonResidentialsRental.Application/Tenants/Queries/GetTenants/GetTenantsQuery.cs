using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenants;

public sealed record GetTenantsQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<TenantResponse>>;
