using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeBillingSettings;

public sealed record ChangeBillingSettingsCommand(
    LeaseId LeaseId,
    int NewDueDayOfMonth,
    int NewGracePeriodDays) : ICommand<Unit>;
