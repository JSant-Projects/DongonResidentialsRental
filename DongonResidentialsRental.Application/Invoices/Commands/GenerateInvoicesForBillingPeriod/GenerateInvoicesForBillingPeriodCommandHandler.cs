using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;

public sealed class GenerateInvoicesForBillingPeriodCommandHandler : 
    ICommandHandler<GenerateInvoicesForBillingPeriodCommand, GenerateInvoicesForBillingPeriodResult>
{
    private const string RENT_LINE_DESCRIPTION = "Monthly Rent";
    private const int DEFAULT_RENT_QUANTITY = 1;

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IApplicationDBContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    public GenerateInvoicesForBillingPeriodCommandHandler(
        IInvoiceRepository invoiceRepository,
        IApplicationDBContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ILeaseRepository leaseRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _invoiceRepository = invoiceRepository;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _leaseRepository = leaseRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }
    public async Task<GenerateInvoicesForBillingPeriodResult> Handle(GenerateInvoicesForBillingPeriodCommand request, CancellationToken cancellationToken)
    {

        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Get leases active for at least part of that month. 
        var leases = await _leaseRepository.GetLeasesOverlappingPeriodAsync(
                                                monthStart, 
                                                monthEnd, 
                                                cancellationToken);

        int totalEvaluated = 0;
        int totalCreated = 0;
        int totalSkipped = 0;

        foreach (var lease in leases)
        {
            totalEvaluated++;

            var billingFrom = lease.Term.StartDate > monthStart
                ? lease.Term.StartDate
                : monthStart;

            var billingTo = (lease.Term.EndDate is not null && lease.Term.EndDate < monthEnd)
                ? (DateOnly)lease.Term.EndDate
                : monthEnd;

            var billingPeriod = BillingPeriod.Create(billingFrom, billingTo);

            // Check if issued invoice for the lease and current billing period exists
            var issuedInvoiceExists = await _invoiceRepository.ExistsIssuedAsync(
                                                lease.LeaseId, 
                                                billingPeriod, 
                                                cancellationToken);

            if (issuedInvoiceExists)
            {
                totalSkipped++;
                continue;
            }

            var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);
            var monthlyRent = lease.MonthlyRate;

            var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

            var invoice = Invoice.Create(
                invoiceNumber,
                lease.LeaseId, 
                billingPeriod, 
                dueDate, 
                lease.MonthlyRate.Currency);


            invoice.AddLine(RENT_LINE_DESCRIPTION, DEFAULT_RENT_QUANTITY, monthlyRent, InvoiceLineType.Rent);

            _invoiceRepository.Add(invoice);
            totalCreated++;
        }

        return new GenerateInvoicesForBillingPeriodResult(
            totalEvaluated,
            totalCreated,
            totalSkipped);
    }
}
