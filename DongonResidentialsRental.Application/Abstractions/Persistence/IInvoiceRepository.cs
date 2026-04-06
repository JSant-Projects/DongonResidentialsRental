using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Abstractions.Persistence;

public interface IInvoiceRepository
{
    void Add(Invoice invoice);
    Task<Invoice?> GetByIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetWithDetailsByIAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsIssuedAsync(
        LeaseId leaseId,
        BillingPeriod billingPeriod,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<LeaseId, BillingPeriod?>> GetLatestBillingPeriodsByLeaseIdsAsync(
        IReadOnlyCollection<LeaseId> leaseIds,
        CancellationToken cancellationToken = default);
    void Remove(Invoice invoice);
}
