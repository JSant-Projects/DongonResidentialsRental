using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;

public sealed record GenerateInvoicesForBillingPeriodResult(
    int TotalEvaluated, 
    int TotalCreated, 
    int TotalSkipped);
