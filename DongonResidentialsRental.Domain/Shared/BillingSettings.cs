using DongonResidentialsRental.Domain.Invoice;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record BillingSettings
{
    public int DueDayOfMonth { get; }
    public int GracePeriodDays { get; }

    private BillingSettings(int dueDayOfMonth, int gracePeriodDays)
    {
        DueDayOfMonth = dueDayOfMonth;
        GracePeriodDays = gracePeriodDays;
    }

    public static BillingSettings Create(int dueDayOfMonth, int gracePeriodDays)
    {
        Ensure.InRangeInteger(dueDayOfMonth, 1, 28, "Due day of month must be between 1 and 28");
        Ensure.NonNegativeInteger(gracePeriodDays, "Grace period days cannot be negative");
        return new BillingSettings(dueDayOfMonth, gracePeriodDays);
    }

    public DateOnly CalculateDueDate(BillingPeriod billingPeriod)
    {
        return new DateOnly(
            billingPeriod.From.Year,
            billingPeriod.From.Month,
            DueDayOfMonth);
    }

    public DateOnly CalculateLateAfterDate(BillingPeriod billingPeriod)
    {
        return CalculateDueDate(billingPeriod).AddDays(GracePeriodDays);
    }
}
