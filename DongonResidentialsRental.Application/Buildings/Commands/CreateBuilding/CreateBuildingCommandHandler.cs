using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;

public sealed class CreateBuildingCommandHandler : ICommandHandler<CreateBuildingCommand, BuildingId>
{
    private readonly IBuildingRepository _buildingRepository;
    public CreateBuildingCommandHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }
    public async Task<BuildingId> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
                            request.AddressStreet,
                            request.AddressCity,
                            request.AddressProvince,
                            request.AddressPostalCode);

        bool isBuildingNameExists = await _buildingRepository.ExistsByNameAsync(
                                            request.Name,
                                            cancellationToken);

        if (isBuildingNameExists)
        {
            throw new ConflictException($"A building with the name {request.Name} already exists.");
        }

        var building = Building.Create(request.Name, address);

        _buildingRepository.Add(building);

        return building.BuildingId;
    }
}
