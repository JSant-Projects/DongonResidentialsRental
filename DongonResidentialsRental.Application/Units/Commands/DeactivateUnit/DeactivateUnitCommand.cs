using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Unit;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.DeactivateUnit;

public sealed record DeactivateUnitCommand(UnitId UnitId) : ICommand<Unit>;
