using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Queries;

public sealed record LeaseResponse(
    LeaseId LeaseId,
    string Building,
    string UnitNumber,
    string TenantName,
    decimal MonthlyRate);
