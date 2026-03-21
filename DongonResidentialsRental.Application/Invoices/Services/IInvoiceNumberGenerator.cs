using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}
