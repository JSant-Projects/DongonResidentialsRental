using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsIssuedAsync(
        LeaseId leaseId,
        BillingPeriod billingPeriod,
        CancellationToken cancellationToken = default);
    Task RemoveAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
