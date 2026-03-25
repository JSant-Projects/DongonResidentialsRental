using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.CreditNote;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.CreateCreditNote;

public sealed record CreateCreditNoteCommand(
    LeaseId LeaseId,
    decimal Amount,
    string Currency) : ICommand<CreditNoteId>;
