using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands;

public sealed record GenerateInvoicesResult(
    int TotalEvaluated, 
    int TotalCreated, 
    int TotalSkipped);
