using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Invoices.Policies;

public sealed class InvoiceIssuancePolicy : IInvoiceIssuancePolicy
{
    public void EnsureCanIssue(Invoice invoice, Lease lease)
    {
        if (lease.UtilityResponsibility.TenantPaysElectricity &&
            !invoice.HasLineOfType(InvoiceLineType.Electricity))
        {
            throw new DomainException("Electricity line is required before issuing this invoice.");
        }


        if (lease.UtilityResponsibility.TenantPaysWater &&
            !invoice.HasLineOfType(InvoiceLineType.Water))
        {
            throw new DomainException("Water line is required before issuing this invoice.");
        }
    }
}
