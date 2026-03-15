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
    public Task<BuildingId> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
                            request.AddressStreet,
                            request.AddressCity,
                            request.AddressProvince,
                            request.AddressPostalCode);

        var building = Building.Create(request.Name, address);

        _buildingRepository.Add(building);

        return Task.FromResult(building.BuildingId);
    }
}
