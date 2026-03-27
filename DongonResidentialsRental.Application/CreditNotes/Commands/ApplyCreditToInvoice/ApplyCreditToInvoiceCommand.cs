using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.ApplyCreditToInvoice;

public sealed record ApplyCreditToInvoiceCommand(
    CreditNoteId CreditNoteId,
    InvoiceId InvoiceId,
    decimal Amount) : ICommand<Unit>;
