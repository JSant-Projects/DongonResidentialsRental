namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record CreateLeaseRequest(
    Guid Occupancy,
    Guid UnitId,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal MonthlyRate,
    int DueDayOfMonth,
    int GracePeridoDays,
    bool tenantPaysElectricity,
    bool tenantPaysWater,
    string Currency);
