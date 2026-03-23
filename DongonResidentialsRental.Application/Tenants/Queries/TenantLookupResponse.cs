using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries;

public sealed record TenantLookupResponse(
    Guid TenantId, 
    string FullName);
