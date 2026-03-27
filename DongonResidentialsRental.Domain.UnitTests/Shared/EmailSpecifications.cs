using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.UnitTests.Shared;

public class EmailSpecifications
{
    [Theory]
    [InlineData("jayson@sample.com")]
    [InlineData("belle@sample.com")]
    [InlineData("cathy@sample.com")]
    public void Create_Should_Return_Email_When_Email_Is_Valid(string email)
    {
        var result = Email.Create(email);

        result.Should().BeOfType<Email>();
        result.Should().NotBeNull();
        result.Value.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_Should_Throw_ArgumentException_When_Email_Is_Empty_Or_Null(string email)
    {
        Action act = () => Email.Create(email);

        act.Should().ThrowExactly<ArgumentException>().WithMessage("Email cannot be null or empty*");
    }


    [Theory]
    [InlineData("jayson@.com")]
    [InlineData(".com")]
    [InlineData("testdata?")]
    public void Create_Should_Throw_DomainException_When_Email_Is_Invalid(string email)
    {
        Action act = () => Email.Create(email);

        act.Should().ThrowExactly<DomainException>().WithMessage("Invalid email");
    }
}
