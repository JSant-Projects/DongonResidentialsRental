using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Persistence.Repositories;

internal sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;
    public InvoiceRepository(ApplicationDbContext dbContext )
    {
        _dbContext = dbContext;
    }
    public void Add(Invoice invoice)
    {
        _dbContext.Invoices.Add(invoice);
    }

    public async Task<bool> ExistsIssuedAsync(
        LeaseId leaseId, 
        BillingPeriod billingPeriod, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
                        .AnyAsync(i => 
                            i.LeaseId == leaseId &&
                            i.BillingPeriod.From == billingPeriod.From &&
                            i.BillingPeriod.To == billingPeriod.To,
                            cancellationToken);
    }

    public async Task<Invoice?> GetByIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
                        .FirstOrDefaultAsync(i => 
                            i.InvoiceId == invoiceId, 
                            cancellationToken);
    }

    public async Task<IReadOnlyDictionary<LeaseId, BillingPeriod?>> GetLatestBillingPeriodsByLeaseIdsAsync(
        IReadOnlyCollection<LeaseId> leaseIds, 
        CancellationToken cancellationToken = default)
    {
        if (leaseIds.Count == 0)
        {
            return new Dictionary<LeaseId, BillingPeriod?>();
        }

        var latestPeriods = await _dbContext.Invoices
                                    .Where(i => leaseIds.Contains(i.LeaseId))
                                    .GroupBy(i => i.LeaseId)
                                    .Select(g => new
                                    {
                                        LeaseId = g.Key,
                                        BillingPeriod = g
                                            .OrderByDescending(i => i.BillingPeriod.To)
                                            .ThenByDescending(i => i.BillingPeriod.From)
                                            .Select(i => i.BillingPeriod)
                                            .FirstOrDefault()
                                    })
                                    .ToDictionaryAsync(
                                        x => x.LeaseId,
                                        x => (BillingPeriod?)x.BillingPeriod,
                                        cancellationToken);

        return latestPeriods;
    }

    public void Remove(Invoice invoice)
    {
        _dbContext.Invoices.Remove(invoice);
    }
}
