using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Tests.Domain.Shared;

public class PostalCodeSpecifications
{
    [Theory]
    [InlineData("1234")]
    [InlineData("5678")]
    [InlineData("9012")]
    public void Create_Should_Return_PostalCode_When_PostalCode_Is_Valid(string postaCode)
    {
        var result = PostalCode.Create(postaCode);
        result.Should().NotBeNull();
        result.Should().BeOfType<PostalCode>();
        result.Value.Should().Be(postaCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_Should_Throw_ArgumentException_When_PostalCode_Is_Empty_Or_Null(string postaCode)
    {
        var act = () => PostalCode.Create(postaCode);
        act.Should().ThrowExactly<ArgumentException>().WithMessage("Postal code cannot be null or empty*");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("abcd")]
    public void Create_Should_Throw_DomainException_When_PostalCode_Is_Invalid(string postaCode)
    {
        var act = () => PostalCode.Create(postaCode);
        act.Should().ThrowExactly<DomainException>().WithMessage("Invalid postal code*");
    }
}
