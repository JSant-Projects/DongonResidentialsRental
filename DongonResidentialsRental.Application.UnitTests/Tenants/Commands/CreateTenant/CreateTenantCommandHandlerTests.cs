using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Application.Tenants.Commands.CreateTenant;
using DongonResidentialsRental.Domain.Tenant;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.UnitTests.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository = Substitute.For<ITenantRepository>();

    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _handler = new CreateTenantCommandHandler(_tenantRepository);
    }

    [Fact]
    public async Task Handle_Should_Throw_ConflictException_When_Tenant_With_Email_Already_Exists()
    {
        // Arrange
        var command = new CreateTenantCommand(
            "Jayson",
            "Santiago",
            "jayson@email.com",
            "09124567890");

        _tenantRepository
            .ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Tenant with email {command.Email} already exists");

        _tenantRepository.DidNotReceive()
            .Add(Arg.Any<Tenant>());
    }

    [Fact]
    public async Task Handle_Should_Create_Tenant_Add_To_Repository_And_Return_TenantId_When_Request_Is_Valid()
    {
        // Arrange
        var command = new CreateTenantCommand(
            "Jayson",
            "Santiago",
            "jayson@email.com",
            "09124567890");

        _tenantRepository
            .ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        Tenant? addedTenant = null;

        _tenantRepository
            .When(x => x.Add(Arg.Any<Tenant>()))
            .Do(callInfo => addedTenant = callInfo.Arg<Tenant>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(default);

        addedTenant.Should().NotBeNull();
        addedTenant!.TenantId.Should().Be(result);
        addedTenant.PersonalInfo.FirstName.Should().Be("Jayson");
        addedTenant.PersonalInfo.LastName.Should().Be("Santiago");
        addedTenant.ContactInfo.Email.Value.Should().Be("jayson@email.com");
        addedTenant.ContactInfo.PhoneNumber.Value.Should().Be("09124567890");

        _tenantRepository.Received(1)
            .Add(Arg.Any<Tenant>());
    }

    [Fact]
    public async Task Handle_Should_Create_Tenant_Using_Command_Values()
    {
        // Arrange
        var command = new CreateTenantCommand(
            "Maria",
            "Lopez",
            "maria@email.com",
            "09124567890");

        _tenantRepository
            .ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        Tenant? addedTenant = null;

        _tenantRepository
            .When(x => x.Add(Arg.Any<Tenant>()))
            .Do(callInfo => addedTenant = callInfo.Arg<Tenant>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        addedTenant.Should().NotBeNull();
        addedTenant!.PersonalInfo.FirstName.Should().Be("Maria");
        addedTenant.PersonalInfo.LastName.Should().Be("Lopez");
        addedTenant.ContactInfo.Email.Value.Should().Be("maria@email.com");
        addedTenant.ContactInfo.PhoneNumber.Value.Should().Be("09124567890");
    }
}
