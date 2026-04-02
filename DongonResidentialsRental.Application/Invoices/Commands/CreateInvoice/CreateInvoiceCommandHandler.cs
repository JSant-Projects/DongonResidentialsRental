using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceId>
{
    private const string RENT_LINE_DESCRIPTION = "Monthly Rent";
    private const int DEFAULT_RENT_QUANTITY = 1;


    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository, 
        ILeaseRepository leaseRepository,
        IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _invoiceRepository = invoiceRepository;
        _leaseRepository = leaseRepository;
        _invoiceNumberGenerator = invoiceNumberGenerator;
    }
    public async Task<InvoiceId> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }


        var billingPeriod = BillingPeriod.Create(request.Period.From, request.Period.To);

        // Check if issued invoice for the lease and current billing period exists
        var issuedInvoiceExists = await _invoiceRepository.ExistsIssuedAsync(
                                            lease.LeaseId,
                                            billingPeriod,
                                            cancellationToken);

        if (issuedInvoiceExists)
        {
            throw new InvalidOperationException($"Lease {lease.LeaseId} already have an issued invoice for the current billing period.");
        }

        var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);
        var monthlyRent = lease.MonthlyRate;

        var invoiceNumber = await _invoiceNumberGenerator.GenerateAsync(cancellationToken);

        var invoice = Invoice.Create(
            invoiceNumber,
            request.LeaseId, 
            billingPeriod, 
            dueDate, 
            monthlyRent.Currency);

        invoice.AddLine(RENT_LINE_DESCRIPTION, DEFAULT_RENT_QUANTITY, monthlyRent, InvoiceLineType.Rent);

        _invoiceRepository.Add(invoice);

        return invoice.InvoiceId;

    }
}