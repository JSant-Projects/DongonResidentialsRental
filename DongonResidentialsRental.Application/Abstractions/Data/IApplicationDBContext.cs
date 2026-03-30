using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Building> Buildings { get; }
    DbSet<Unit> Units { get; }
    DbSet<Lease> Leases { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<InvoiceAllocation> InvoicesAllocations { get; }
    DbSet<InvoiceCreditAllocation> InvoicesCreditAllocations { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentAllocation> PaymentsAllocations { get; }
    DbSet<CreditNote> CreditNotes { get; }
    DbSet<CreditAllocation> CreditAllocations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
