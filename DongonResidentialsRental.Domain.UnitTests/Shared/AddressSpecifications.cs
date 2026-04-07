using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.UnitTests.Shared;

public class AddressSpecifications
{
    [Theory]
    [InlineData("123 Main St", "Muntinlupa", "Manila", "1234")]
    [InlineData("245 Main St", "Paranaque", "Manila", "1235")]
    [InlineData("678 Main St", "Las Pinas", "Manila", "1236")]

    public void Create_Should_Return_Address_When_Address_Is_Valid(string street, string city, string province, string postalCode)
    {
        var result = Address.Create(street, city, province, postalCode);
        result.Should().BeOfType<Address>();
        result.Should().NotBeNull();
        result.Street.Should().Be(street);
        result.City.Should().Be(city);
        result.Province.Should().Be(province);
        result.PostalCode.Should().Be(postalCode);
    }

    [Theory]
    [InlineData("", "Muntinlupa", "Manila", "1234")]
    [InlineData(null, "Paranaque", "Manila", "1235")]
    public void Create_Should_Throw_DomainException_When_Street_Is_Empty_Or_Null(string street, string city, string province, string postalCode)
    {
        Action act = () => Address.Create(street, city, province, postalCode);
        act.Should().ThrowExactly<DomainException>().WithMessage("Street cannot be null or empty*");
    }

    [Theory]
    [InlineData("123 Main St", "", "Manila", "1234")]
    [InlineData("123 Main St", null, "Manila", "1235")]
    public void Create_Should_Throw_DomainException_When_City_Is_Empty_Or_Null(string street, string city, string province, string postalCode)
    {
        Action act = () => Address.Create(street, city, province, postalCode);
        act.Should().ThrowExactly<DomainException>().WithMessage("City cannot be null or empty*");
    }

    [Theory]
    [InlineData("123 Main St", "Muntinlupa", "", "1234")]
    [InlineData("123 Main St", "Muntinlupa", null, "1235")]
    public void Create_Should_Throw_DomainException_When_Province_Is_Empty_Or_Null(string street, string city, string province, string postalCode)
    {
        Action act = () => Address.Create(street, city, province, postalCode);
        act.Should().ThrowExactly<DomainException>().WithMessage("Province cannot be null or empty*");
    }
}
