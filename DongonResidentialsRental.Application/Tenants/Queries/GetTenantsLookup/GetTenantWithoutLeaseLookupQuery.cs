using DongonResidentialsRental.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantsLookup;

public sealed record GetTenantWithoutLeaseLookupQuery() : IQuery<IReadOnlyList<TenantLookupResponse>>;
