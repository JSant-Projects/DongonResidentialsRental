using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeMonthlyRate;

public sealed record ChangeMonthlyRateCommand(
    LeaseId LeaseId, 
    decimal NewMonthlyRate,
    string Currency) : ICommand<Unit>;
