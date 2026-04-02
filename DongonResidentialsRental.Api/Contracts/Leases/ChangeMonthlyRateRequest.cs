namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record ChangeMonthlyRateRequest(
    decimal NewMonthlyRate,
    string Currency);
