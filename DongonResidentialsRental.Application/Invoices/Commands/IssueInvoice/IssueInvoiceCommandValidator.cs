using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId.Id)
            .NotEmpty();
    }
}