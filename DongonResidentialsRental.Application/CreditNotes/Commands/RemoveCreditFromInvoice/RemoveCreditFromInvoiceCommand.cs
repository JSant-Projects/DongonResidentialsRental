using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.RemoveCreditFromInvoice;

public sealed record RemoveCreditFromInvoiceCommand(
    CreditNoteId CreditNoteId,
    InvoiceId InvoiceId) : ICommand<Unit>;
