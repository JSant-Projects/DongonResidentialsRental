using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.CreateUnit;

public sealed record CreateUnitCommand(
    BuildingId BuildingId, 
    string UnitNumber, 
    int? Floor) : ICommand<UnitId>;
