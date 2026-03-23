using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.ActivateLease;

public sealed record ActivateLeaseCommand(LeaseId LeaseId) : ICommand<Unit>;
