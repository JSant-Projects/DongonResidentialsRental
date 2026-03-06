using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Tests.Shared;

public class PhoneNumberSpecifications
{
    [Theory]
    [InlineData("09171234567")]
    [InlineData("+639171234567")]
    public void Create_Should_Return_PhoneNumber_When_PhoneNumber_Is_Valid(string phoneNumber)
    {
        var result = PhoneNumber.Create(phoneNumber);
        result.Should().BeOfType<PhoneNumber>();
        result.Should().NotBeNull();
        result.Value.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_Should_Throw_ArgumentException_When_PhoneNumber_Is_Empty_Or_Null(string phoneNumber)
    {
        Action act = () => PhoneNumber.Create(phoneNumber);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("PhoneNumber cannot be null or empty*");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("+?639171234567")]
    [InlineData("0917123456799")]
    public void Create_ShouldNot_Throw_ArgumentException_When_PhoneNumber_Is_InvalidFormat(string phoneNumber)
    {
        Action act = () => PhoneNumber.Create(phoneNumber);
        act.Should().ThrowExactly<DomainException>().WithMessage("Invalid phone number*");
    }

}
