using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.CreditNote;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.CreditNotes.Commands.IssueCreditNote;

public sealed record IssueCreditNoteCommand(CreditNoteId CreditNoteId) : ICommand<Unit>;
