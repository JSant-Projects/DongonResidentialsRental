using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.CreateLease;

public sealed record CreateLeaseCommand(
    TenantId Occupancy, 
    UnitId UnitId,
    DateOnly StartDate,
    DateOnly? EndDate,
    Decimal MonthlyRate,
    string Currency) : ICommand<LeaseId>;
