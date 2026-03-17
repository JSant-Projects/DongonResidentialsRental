using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeLeaseTerm;

public sealed record ChangeLeaseTermCommand(
    LeaseId LeaseId, 
    DateOnly NewStartDate, 
    DateOnly? NewEndDate) : ICommand<Unit>;
