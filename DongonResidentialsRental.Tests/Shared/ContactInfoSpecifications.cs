using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Tests.Shared;

public class ContactInfoSpecifications
{
    [Theory]
    [InlineData("jayson.santiago@sample.com", "09171234567")]
    [InlineData("cathy.dongon@sample.com", "09181234567")]
    [InlineData("belle.santiago@sample.com", "+639191234567")]
    public void Create_Should_Return_ContactInfo_When_Email_And_PhoneNumber_Are_Valid(string contactEmail, string contanctPhoneNumber)
    {
        var email = Email.Create(contactEmail);
        var phoneNumber = PhoneNumber.Create(contanctPhoneNumber);
        var result = ContactInfo.Create(email, phoneNumber);
        result.Should().NotBeNull();
        result.Should().BeOfType<ContactInfo>();
        result.Email.Should().Be(email);
        result.PhoneNumber.Should().Be(phoneNumber);
    }
}
