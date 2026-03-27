using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Application.Invoices.Commands.CancelInvoice;

public sealed class CancelInvoiceCommandHandler : ICommandHandler<CancelInvoiceCommand, Unit>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public CancelInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }
    public async Task<Unit> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        invoice.Cancel();

        return Unit.Value;
    }
}
