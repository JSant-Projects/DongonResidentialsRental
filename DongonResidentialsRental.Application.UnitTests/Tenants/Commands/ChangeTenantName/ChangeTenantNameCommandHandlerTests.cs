using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantName;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Tenants.Commands.ChangeTenantName;

public sealed class ChangeTenantNameCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();

    private readonly ChangeTenantNameCommandHandler _handler;

    public ChangeTenantNameCommandHandlerTests()
    {
        _handler = new ChangeTenantNameCommandHandler(_tenantRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Tenant_Does_Not_Exist()
    {
        // Arrange
        var tenantId = NewTenantId();

        var command = new ChangeTenantNameCommand(
            tenantId,
            "Maria",
            "Lopez");

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
    public async Task Handle_Should_Change_Tenant_Name_When_Tenant_Exists()
    {
        // Arrange
        var tenant = CreateTenant(
            firstName: "Jayson",
            lastName: "Santiago",
            email: "jayson@email.com",
            phoneNumber: "09123456789");

        var command = new ChangeTenantNameCommand(
            tenant.TenantId,
            "Maria",
            "Lopez");

        _tenantRepository
            .GetByIdAsync(tenant.TenantId, Arg.Any<CancellationToken>())
            .Returns(tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        tenant.PersonalInfo.FirstName.Should().Be("Maria");
        tenant.PersonalInfo.LastName.Should().Be("Lopez");
    }

    [Fact]
    public async Task Handle_Should_Return_UnitValue_When_Successful()
    {
        // Arrange
        var tenant = CreateTenant(
            firstName: "Jayson",
            lastName: "Santiago",
            email: "jayson@email.com",
            phoneNumber: "09123456789");

        var command = new ChangeTenantNameCommand(
            tenant.TenantId,
            "Maria",
            "Lopez");

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
        var phoneNumberVo = PhoneNumber.Create(phoneNumber);
        var contactInfo = ContactInfo.Create(emailVo, phoneNumberVo);

        return Tenant.Create(personalInfo, contactInfo);
    }
}
