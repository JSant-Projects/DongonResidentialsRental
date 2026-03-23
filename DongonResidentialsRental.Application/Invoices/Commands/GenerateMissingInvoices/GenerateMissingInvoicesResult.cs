using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;

public sealed record GenerateMissingInvoicesResult(
    int TotalLeasesEvaluated,
    int TotalInvoicesCreated);
