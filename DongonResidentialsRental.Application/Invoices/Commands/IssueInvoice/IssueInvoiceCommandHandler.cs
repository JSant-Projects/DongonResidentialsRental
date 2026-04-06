using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Invoices.Policies;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler : ICommandHandler<IssueInvoiceCommand, Unit>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInvoiceIssuancePolicy _invoiceIssuancePolicy;
    public IssueInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IDateTimeProvider dateTimeProvider,
        ILeaseRepository leaseRepository,
        IInvoiceIssuancePolicy invoiceIssuancePolicy)
    {
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
        _leaseRepository = leaseRepository;
        _invoiceIssuancePolicy = invoiceIssuancePolicy;
    }
    public async Task<Unit> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        var lease = await _leaseRepository.GetByIdAsync(invoice.LeaseId, cancellationToken);
        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), invoice.LeaseId);
        }

        _invoiceIssuancePolicy.EnsureCanIssue(invoice, lease);

        invoice.Issue(today);

        return Unit.Value;
    }
}
