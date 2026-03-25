using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.RemoveCreditFromInvoice;

public sealed class RemoveCreditFromInvoiceCommandHandler : ICommandHandler<RemoveCreditFromInvoiceCommand, Unit>
{
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    public RemoveCreditFromInvoiceCommandHandler(
        ICreditNoteRepository creditNoteRepository,
        IInvoiceRepository invoiceRepository)
    {
        _creditNoteRepository = creditNoteRepository;
        _invoiceRepository = invoiceRepository;
    }
    public async Task<Unit> Handle(RemoveCreditFromInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        if (invoice is null)
        {
            throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        var creditNote = await _creditNoteRepository.GetByIdAsync(request.CreditNoteId);
        if (creditNote is null)
        {
            throw new NotFoundException(nameof(CreditNote), request.CreditNoteId);
        }

        creditNote.RemoveAllocation(invoice.InvoiceId);
        invoice.RemoveCreditAllocation(creditNote.CreditNoteId);

        return Unit.Value;
    }
}
