using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Payment;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : 
        base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        List<IDomainEvent> domainEvents = ChangeTracker
           .Entries<AggregateRoot>()
           .Select(static entry => entry.Entity)
           .SelectMany(static aggregate => aggregate.DomainEvents)
           .ToList();

        return domainEvents;
    }

    public void ClearDomainEvents()
    {
        IEnumerable<EntityEntry<AggregateRoot>> aggregateEntries = ChangeTracker
            .Entries<AggregateRoot>();

        foreach (EntityEntry<AggregateRoot> aggregateEntry in aggregateEntries)
        {
            aggregateEntry.Entity.ClearDomainEvents();
        }
    }

    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Lease> Leases => Set<Lease>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<InvoiceAllocation> InvoicesAllocations => Set<InvoiceAllocation>();

    public DbSet<InvoiceCreditAllocation> InvoicesCreditAllocations => Set<InvoiceCreditAllocation>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentAllocation> PaymentsAllocations => Set<PaymentAllocation>();

    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();

    public DbSet<CreditAllocation> CreditAllocations => Set<CreditAllocation>();
}
