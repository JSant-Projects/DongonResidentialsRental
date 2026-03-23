using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;

public sealed class GetInvoicesQueryValidator : AbstractValidator<GetInvoicesQuery>
{
    public GetInvoicesQueryValidator()
    {
        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

       RuleFor(x => x.Period)
            .Must(r => r is null || r.From <= r.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}

