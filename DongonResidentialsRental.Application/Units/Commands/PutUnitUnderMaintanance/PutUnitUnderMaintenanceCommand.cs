using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Unit;
using Unit = DongonResidentialsRental.Application.Abstractions.Messaging.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.PutUnitUnderMaintanance;

public sealed record PutUnitUnderMaintenanceCommand(UnitId UnitId) : ICommand<Unit>;
