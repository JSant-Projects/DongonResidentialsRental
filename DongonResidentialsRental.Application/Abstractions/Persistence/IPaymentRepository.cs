using DongonResidentialsRental.Domain.Payment;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Task<Payment?> GetByIdAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
    void Remove(Payment payment);
}
