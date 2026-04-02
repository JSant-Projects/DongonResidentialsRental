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
    decimal MonthlyRate,
    int DueDayOfMonth,
    int GracePeridoDays,
    bool tenantPaysElectricity,
    bool tenantPaysWater,
    string Currency) : ICommand<LeaseId>;
