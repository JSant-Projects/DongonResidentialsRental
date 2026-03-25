using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.CancelInvoice;

public sealed class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId.Id)
           .NotEmpty();
    }
}
