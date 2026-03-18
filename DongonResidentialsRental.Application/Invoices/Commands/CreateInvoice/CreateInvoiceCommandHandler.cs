using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceId>
{
    private const string RENT_LINE_DESCRIPTION = "Monthly Rent";
    private const int DEFAULT_RENT_QUANTITY = 1;


    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILeaseRepository _leaseRepository;
    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository, 
        ILeaseRepository leaseRepository)
    {
        _invoiceRepository = invoiceRepository;
        _leaseRepository = leaseRepository;
    }
    public async Task<InvoiceId> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }


        var billingPeriod = BillingPeriod.Create(request.From, request.To);
        var dueDate = lease.BillingSettings.CalculateDueDate(billingPeriod);
        var monthlyRent = lease.MonthlyRate;

        var invoice = Invoice.Create(
            request.LeaseId, 
            billingPeriod, 
            dueDate, 
            monthlyRent.Currency);

        invoice.AddLine(RENT_LINE_DESCRIPTION, DEFAULT_RENT_QUANTITY, monthlyRent, InvoiceLineType.Rent);

        _invoiceRepository.Add(invoice);

        return invoice.InvoiceId;

    }
}