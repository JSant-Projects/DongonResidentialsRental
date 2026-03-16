using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Unit;

public sealed class Unit
{
    public UnitId UnitId { get; }
    public BuildingId BuildingId { get; }
    public string UnitNumber { get; private set; }  // e.g. "12B"
    public int? Floor { get; private set; }
    public UnitStatus Status { get; private set; }

    private Unit() { }
    private Unit(BuildingId buildingId, string unitNumber, int? floor)
    {
        UnitId = new UnitId(Guid.NewGuid());
        BuildingId = buildingId;
        UnitNumber = unitNumber;
        Floor = floor;
        Status = UnitStatus.Active;
    }

    public void PutUnderMaintenance() 
    {
        if (Status == UnitStatus.Inactive)
            throw new DomainException("Cannot put an inactive unit under maintenance.");

        Status = UnitStatus.Maintenance; 
    }
    public void Activate() => Status = UnitStatus.Active;
    public void Deactivate() => Status = UnitStatus.Inactive;


    public static Unit Create(BuildingId buildingId, string unitNumber, int? floor = null)
    {
        Ensure.NotNull(buildingId, "BuildingId cannot be null");
        Ensure.NotNullOrWhiteSpace(unitNumber, "Unit number cannot be null or empty");

        return new Unit(buildingId, unitNumber.Trim(), floor);
    }


}
