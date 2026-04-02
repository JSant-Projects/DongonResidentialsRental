namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record ChangeLeaseTermRequest(
    DateOnly NewStartDate,
    DateOnly? NewEndDate);
