using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoices;

public sealed class GetOutstandingInvoicesQueryValidator : AbstractValidator<GetOutstandingInvoicesQuery>
{
    public GetOutstandingInvoicesQueryValidator()
    {
        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}