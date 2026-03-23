using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.TerminateLease;

public sealed record TerminateLeaseCommand(
    LeaseId LeaseId, 
    DateOnly TerminationDate) : ICommand<Unit>;
