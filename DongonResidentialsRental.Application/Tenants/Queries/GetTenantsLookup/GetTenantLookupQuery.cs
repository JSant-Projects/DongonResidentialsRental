using DongonResidentialsRental.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantsLookup;

public sealed record GetTenantLookupQuery() : IQuery<IReadOnlyList<TenantLookupResponse>>;
