namespace DongonResidentialsRental.Api.Contracts.Buildings;

public sealed record CreateBuildingRequest(
    string Name,
    string AddressStreet,
    string AddressCity,
    string AddressProvince,
    string AddressPostalCode);
