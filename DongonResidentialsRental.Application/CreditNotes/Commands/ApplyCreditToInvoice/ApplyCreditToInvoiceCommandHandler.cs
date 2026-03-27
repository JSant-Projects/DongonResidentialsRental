using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.ApplyCreditToInvoice;

public sealed class ApplyCreditToInvoiceCommandHandler : ICommandHandler<ApplyCreditToInvoiceCommand, Unit>
{
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ApplyCreditToInvoiceCommandHandler(
        ICreditNoteRepository creditNoteRepository,
        IInvoiceRepository invoiceRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _creditNoteRepository = creditNoteRepository;
        _invoiceRepository = invoiceRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(ApplyCreditToInvoiceCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

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

        var amount = Money.Create(creditNote.Amount.Currency, request.Amount);

        creditNote.ApplyToInvoice(invoice.InvoiceId, amount, today);
        invoice.ApplyCredit(creditNote.CreditNoteId, amount, today);

        return Unit.Value;
    }
}
