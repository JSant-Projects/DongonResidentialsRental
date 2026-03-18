using DongonResidentialsRental.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;

public sealed record GenerateInvoicesForBillingPeriodCommand(
    int Year, 
    int Month) : ICommand<GenerateInvoicesResult>;
