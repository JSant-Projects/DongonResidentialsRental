using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Api.Contracts.Payments;

public sealed record CreatePayment(
    Guid TenantId,
    decimal Amount,
    string Currency,
    DateOnly ReceivedOn,
    PaymentMethod PaymentMethod,
    string? Reference);
