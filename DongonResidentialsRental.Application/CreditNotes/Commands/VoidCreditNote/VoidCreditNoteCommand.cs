using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.CreditNote;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.VoidCreditNote;

public sealed record VoidCreditNoteCommand(CreditNoteId CreditNoteId) : ICommand<Unit>;
