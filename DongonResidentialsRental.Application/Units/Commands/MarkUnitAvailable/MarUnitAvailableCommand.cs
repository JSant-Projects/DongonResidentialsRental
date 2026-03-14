using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.MarkUnitAvailable;

public sealed record MarUnitAvailableCommand(UnitId UnitId) : ICommand<Unit>;
