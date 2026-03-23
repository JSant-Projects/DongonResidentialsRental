using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeUtilityResponsibility;

public sealed record ChangeUtilityResponsibilityCommand(
    LeaseId LeaseId, 
    bool TenantPaysElectricity, 
    bool TenantPaysWater) : ICommand<Unit>;
