using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;

public class CreateBuildingCommandHandler : ICommandHandler<CreateBuildingCommand, BuildingId>
{
    private readonly IBuildingRepository _buildingRepository;
    public CreateBuildingCommandHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }
    public async Task<BuildingId> Handle(CreateBuildingCommand command, CancellationToken cancellationToken)
    {
        var address = Address.Create(
                            command.AddressStreet, 
                            command.AddressCity, 
                            command.AddressProvince, 
                            command.AddressPostalCode);

        var building = Building.Create(command.Name, address);

        await _buildingRepository.AddAsync(building, cancellationToken);

        return building.BuildingId;
    }
}
