using FluentValidation;

namespace DongonResidentialsRental.Application.Payments.Queries.GetPaymentsByTenant;

public sealed class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsByTenantQuery>
{
    public GetPaymentsQueryValidator()
    {

        RuleFor(x => x.Page)
           .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
