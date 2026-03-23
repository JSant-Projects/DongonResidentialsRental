using FluentValidation;

namespace DongonResidentialsRental.Application.Invoices.Queries.GetInvoicesDueSoon;

public sealed class GetInvoicesDueSoonQueryValidator : AbstractValidator<GetInvoicesDueSoonQuery>
{
    public GetInvoicesDueSoonQueryValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(1, 31);

        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
