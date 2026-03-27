using DongonResidentialsRental.Domain.Shared;
using System.Runtime.InteropServices.Marshalling;
using DomainTenant = DongonResidentialsRental.Domain.Tenant.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Domain.UnitTests.Tenant;

public class TenantSpecifications
{
    // Helpers
    private static PersonalInfo CreatePersonalInfo(string firstName, string lastName) 
        => PersonalInfo.Create(firstName, lastName);

    private static ContactInfo CreateContactInfo(string email, string phoneNumber)
    {
        var emailInfo = Email.Create(email);
        var phoneNumberInfo = PhoneNumber.Create(phoneNumber);
        return ContactInfo.Create(emailInfo, phoneNumberInfo);
    }
    
    private static DomainTenant CreateTenant(PersonalInfo personalInfo, ContactInfo contactInfo) 
        => DomainTenant.Create(personalInfo, contactInfo);


    // Create

    [Theory]
    [InlineData("Jayson", "Santiago", "jayson.santiago@sample.com", "09171234567")]
    [InlineData("Belle", "Santiago", "belle.santiago@sample.com", "+639191234567")]
    [InlineData("Cathy", "Dongon", "cathy.dongon@sample.com", "09181234567")]
    public void Create_Should_Return_Tenant_When_PersonalInfo_And_ContactInfo_Are_Valid(string firstName, string lastName, string email, string phoneNumber)
    {
        // Arrange
        var personalInfo = CreatePersonalInfo(firstName, lastName);
        var contactInfo = CreateContactInfo(email, phoneNumber);
        var result = CreateTenant(personalInfo, contactInfo);
        result.Should().NotBeNull();
        result.Should().BeOfType<DomainTenant>();
        result.PersonalInfo.Should().Be(personalInfo);
        result.ContactInfo.Should().Be(contactInfo);
    }

    [Fact]
    public void Create_Should_Throw_ArgumentException_When_PersonalInfo_Is_Null()
    {
        // Arrange
        PersonalInfo personalInfo = null;
        var contactInfo = CreateContactInfo("jayson.santiago@sample.com", "09171234567");
        var act = () => CreateTenant(personalInfo, contactInfo);
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Personal info cannot be null*");
    }

    [Fact]
    public void Create_Should_Throw_ArgumentException_When_ContactInfo_Is_Null()
    {
        // Arrange
        var personalInfo = CreatePersonalInfo("Jayson", "Santiago");
        ContactInfo contactInfo = null;
        var act = () => CreateTenant(personalInfo, contactInfo);
        act.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Contact info cannot be null*");
    }
}
