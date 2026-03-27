using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using DomainBuilding = DongonResidentialsRental.Domain.Building.Building;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.UnitTests.Building;

public class BuildingSpecifications
{

    [Theory]
    [InlineData("Sunrise Apartments", "123 Main St", "Muntinlupa", "Manila", "1234")]
    [InlineData("Sunset Villas", "245 Main St", "Paranaque", "Manila", "1235")]
    [InlineData("Oceanview Condos", "678 Main St", "Las Pinas", "Manila", "1236")]

    public void Create_Should_Return_Building_When_Name_And_Address_Are_Valid(string buildingName, string street, string city, string province, string postalCode)
    {
        // Arrange
        var address = Address.Create(street, city, province, postalCode);
        var result =  DomainBuilding.Create(buildingName, address);
        result.Should().NotBeNull();
        result.Should().BeOfType<DomainBuilding>();
        result.Name.Should().Be(buildingName);
        result.Address.Should().Be(address);
    }
}
