using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Api.Contracts.Leases;

public sealed record ChangeBillingSettingsRequest(
    int NewDueDayOfMonth,
    int NewGracePeriodDays);
