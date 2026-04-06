using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Invoices.Policies;

public interface IInvoiceIssuancePolicy
{
    void EnsureCanIssue(Invoice invoice, Lease lease);
}
