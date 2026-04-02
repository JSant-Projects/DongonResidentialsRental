using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.LeaseId.Id)
            .NotEmpty();

        RuleFor(x => x.Period)
            .Must(r => r is null || r.From <= r.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}