using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantContactInfo;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Tenants.Commands.ChangeTenantContactInfo;

public sealed class ChangeTenantContactInfoCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();

    private readonly ChangeTenantContactInfoCommandHandler _handler;

    public ChangeTenantContactInfoCommandHandlerTests()
    {
        _handler = new ChangeTenantContactInfoCommandHandler(_tenantRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Tenant_Does_Not_Exist()
    {
        // Arrange
        var tenantId = NewTenantId();

        var command = new ChangeTenantContactInfoCommand(
            tenantId,
            "new@email.com",
            "09123456789"); // ✅ valid PH format

        _tenantRepository
            .GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"*{tenantId}*");
    }

    [Fact]
    public async Task Handle_Should_Change_ContactInfo_When_Tenant_Exists()
    {
        // Arrange
        var tenant = CreateTenant(
            firstName: "Jayson",
            lastName: "Santiago",
            email: "old@email.com",
            phoneNumber: "09123456789");

        var command = new ChangeTenantContactInfoCommand(
            tenant.TenantId,
            "new@email.com",
            "+639876543210"); // ✅ valid PH format

        _tenantRepository
            .GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        tenant.ContactInfo.Email.Value.Should().Be("new@email.com");
        tenant.ContactInfo.PhoneNumber.Value.Should().Be("+639876543210");
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var tenant = CreateTenant(
            firstName: "Jayson",
            lastName: "Santiago",
            email: "old@email.com",
            phoneNumber: "09123456789");

        var command = new ChangeTenantContactInfoCommand(
            tenant.TenantId,
            "updated@email.com",
            "09111111111");

        _tenantRepository
            .GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    private static TenantId NewTenantId() => new TenantId(Guid.NewGuid());

    private static Tenant CreateTenant(
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        var personalInfo = PersonalInfo.Create(firstName, lastName);
        var emailVo = Email.Create(email);
        var phoneNumberVo = PhoneNumber.Create(phoneNumber); // 👈 validated by regex
        var contactInfo = ContactInfo.Create(emailVo, phoneNumberVo);

        return Tenant.Create(personalInfo, contactInfo);
    }
}
