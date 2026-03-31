using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler : ICommandHandler<IssueInvoiceCommand, Unit>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public IssueInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        invoice.Issue(today);

        return Unit.Value;
    }
}
