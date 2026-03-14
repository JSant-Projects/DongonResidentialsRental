using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using DomainUnit = DongonResidentialsRental.Domain.Unit.Unit;

namespace DongonResidentialsRental.Application.Units.Commands.CreateUnit;

public class CreateUnitCommandHandler : ICommandHandler<CreateUnitCommand, UnitId>
{
    private readonly IUnitRepository _unitRepository;
    private readonly IBuildingRepository _buildingRepository;

    public CreateUnitCommandHandler(IUnitRepository unitRepository, IBuildingRepository buildingRepository)
    {
        _unitRepository = unitRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<UnitId> Handle(CreateUnitCommand command, CancellationToken cancellationToken)
    {
        bool buildingExists = await _buildingRepository.ExistsAsync(command.BuildingId, cancellationToken);

        if (buildingExists) 
        {
            throw new NotFoundException(nameof(Building), command.BuildingId.Id);
        }

        var unit = DomainUnit.Create(command.BuildingId, command.UnitNumber, command.Floor);

        return unit.UnitId;
    }
}
