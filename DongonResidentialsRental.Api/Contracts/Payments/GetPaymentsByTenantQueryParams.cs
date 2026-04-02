using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Api.Contracts.Payments;

public sealed record GetPaymentsByTenantQueryParams(
    Guid TenantId,
    string? Reference,
    PaymentMethod? Method,
    DateOnly ReceivedOn,
    int Page = 1,
    int PageSize = 20);
