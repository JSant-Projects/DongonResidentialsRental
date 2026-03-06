using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Building;

public sealed class Building
{
    public BuildingId BuildingId { get; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public BuildingStatus Status { get; private set; }
    private Building() { }
    private Building( string name, Address address)
    {
        BuildingId = new BuildingId(Guid.NewGuid());
        Name = name;
        Address = address;
        Status = BuildingStatus.Active;
    }
    public static Building Create(string name, Address address)
    {
        Ensure.NotNullOrWhiteSpace(name, "Building name cannot be null or empty");
        Ensure.NotNull(address, "Address cannot be null or empty");
        return new Building(name, address);
    }
    public void Archive() => Status = BuildingStatus.Archived;

}
