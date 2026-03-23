using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Invoices.Commands.AddInvoiceLine;

public sealed class AddInvoiceLineCommandHandler : ICommandHandler<AddInvoiceLineCommand, Unit>
{
    private readonly IInvoiceRepository _invoiceRepository;
    public AddInvoiceLineCommandHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }
    public async Task<Unit> Handle(AddInvoiceLineCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);

        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        var unitPrice = Money.Create(invoice.Currency, request.Price);

        invoice.AddLine(request.Description, request.Quantity, unitPrice, request.LineType);

        return Unit.Value;
    }
}
