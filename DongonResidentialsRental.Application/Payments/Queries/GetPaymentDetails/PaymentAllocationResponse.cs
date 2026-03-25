using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentDetailsQuery;

public sealed record PaymentAllocationResponse(
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    DateOnly AllocatedOn);
