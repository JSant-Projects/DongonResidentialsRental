using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Building;

namespace DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;

public sealed record CreateBuildingCommand(
    string Name, 
    string AddressStreet, 
    string AddressCity, 
    string AddressProvince,
    string AddressPostalCode): ICommand<BuildingId>;
