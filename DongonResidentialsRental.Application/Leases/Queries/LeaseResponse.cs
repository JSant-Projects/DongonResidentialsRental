using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Queries;

public sealed record LeaseResponse(
    LeaseId LeaseId,
    string BuildingName,
    string UnitNumber,
    string TenantFullName,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal RentAmount,
    string Currency,
    bool IsActive);
