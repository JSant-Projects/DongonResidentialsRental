using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Payment payment, CancellationToken cancellationToken = default);
}
