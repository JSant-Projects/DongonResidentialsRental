namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record TerminateLeaseRequest(
    DateOnly TerminationDate);
