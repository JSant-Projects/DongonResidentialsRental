using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantById;

public sealed record GetTenantByIdQuery(TenantId TenantId) : IQuery<TenantResponse>;
