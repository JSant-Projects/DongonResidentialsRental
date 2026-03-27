using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Domain.Invoice;
using System.ComponentModel.DataAnnotations;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;

public sealed class GenerateMissingInvoicesCommandHandler : ICommandHandler<GenerateMissingInvoicesCommand, GenerateMissingInvoicesResult>
{
    private const string RENT_LINE_DESCRIPTION = "Monthly Rent";
    private const int DEFAULT_RENT_QUANTITY = 1;

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    public GenerateMissingInvoicesCommandHandler(
        IInvoiceRepository invoiceRepository,
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider,
        IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _invoiceRepository = invoiceRepository;
        _leaseRepository = leaseRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }
    public async Task<GenerateMissingInvoicesResult> Handle(GenerateMissingInvoicesCommand request, CancellationToken cancellationToken)
    {
        var today = request.Today;

        var activeLeases = await _leaseRepository.GetActiveLeases(today, cancellationToken);

        if (activeLeases.Count is 0)
        {
            return new GenerateMissingInvoicesResult(0,0);
        }

        var leaseIds = activeLeases
            .Select(x => x.LeaseId)
            .ToList();

        var latestPeriodsByLeaseId = await _invoiceRepository
            .GetLatestBillingPeriodsByLeaseIdsAsync(leaseIds, cancellationToken);

        int totalEvaluated = 0;
        int totalCreated = 0;

        foreach (var lease in activeLeases)
        {
            totalEvaluated++;
            latestPeriodsByLeaseId.TryGetValue(lease.LeaseId, out var latestPeriod);

            var missingPeriods = BillingPeriodCalculator.GetMissingCompletedPeriods(
                leaseStart: lease.Term.StartDate,
                leaseEnd: lease.Term.EndDate,
                latestPeriodEnd: latestPeriod?.To,
                today: today);

            foreach (var billingPeriod in missingPeriods)
            {
                var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);

                var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

                var invoice = Invoice.Create(
                    invoiceNumber,
                    lease.LeaseId,
                    billingPeriod,
                    dueDate,
                    lease.MonthlyRate.Currency);

                invoice.AddLine(
                    RENT_LINE_DESCRIPTION,
                    DEFAULT_RENT_QUANTITY,
                    lease.MonthlyRate,
                    InvoiceLineType.Rent);

                 _invoiceRepository.Add(invoice);

                totalCreated++;
            }

        }

        return new GenerateMissingInvoicesResult(totalEvaluated, totalCreated);
    }
}
