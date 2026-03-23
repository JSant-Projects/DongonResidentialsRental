using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;

public sealed record GenerateMissingInvoicesCommand(
    DateOnly Today) : ICommand<GenerateMissingInvoicesResult>;
