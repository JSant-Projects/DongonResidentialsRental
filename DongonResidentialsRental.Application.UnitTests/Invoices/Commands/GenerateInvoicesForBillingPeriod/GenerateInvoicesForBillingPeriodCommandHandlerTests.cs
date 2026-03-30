using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Invoices.Commands.GenerateInvoicesForBillingPeriod;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using Xunit;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.GenerateInvoicesForBillingPeriod;

public sealed class GenerateInvoicesForBillingPeriodCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = Substitute.For<IInvoiceNumberGenerator>();

    private readonly GenerateInvoicesForBillingPeriodCommandHandler _handler;

    public GenerateInvoicesForBillingPeriodCommandHandlerTests()
    {
        _handler = new GenerateInvoicesForBillingPeriodCommandHandler(
            _invoiceRepository,
            _dbContext,
            _dateTimeProvider,
            _leaseRepository,
            _invoiceNumberGenerator);
    }

    [Fact]
    public async Task Handle_Should_Return_Zeroes_When_No_Overlapping_Leases_Exist()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                Arg.Any<DateOnly>(),
                Arg.Any<DateOnly>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(0);
        result.TotalCreated.Should().Be(0);
        result.TotalSkipped.Should().Be(0);

        _invoiceRepository.DidNotReceive().Add(Arg.Any<Invoice>());
        await _invoiceRepository.DidNotReceive().ExistsIssuedAsync(
            Arg.Any<LeaseId>(),
            Arg.Any<BillingPeriod>(),
            Arg.Any<CancellationToken>());

        await _invoiceNumberGenerator.DidNotReceive().GenerateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Skip_Lease_When_Issued_Invoice_Already_Exists()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Any<BillingPeriod>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(1);
        result.TotalCreated.Should().Be(0);
        result.TotalSkipped.Should().Be(1);

        _invoiceRepository.DidNotReceive().Add(Arg.Any<Invoice>());
        await _invoiceNumberGenerator.DidNotReceive().GenerateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Create_Invoice_When_No_Issued_Invoice_Exists()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null,
            monthlyRentAmount: 1500m,
            currency: "CAD",
            dueDayOfMonth: 5);

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Any<BillingPeriod>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202603-0001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(1);

        // NOTE:
        // This assertion reflects your CURRENT implementation.
        // It will remain 0 because totalCreated is never incremented in the handler.
        result.TotalCreated.Should().Be(1);

        result.TotalSkipped.Should().Be(0);

        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(invoice =>
            invoice.LeaseId == lease.LeaseId &&
            invoice.InvoiceNumber == "INV-202603-0001" &&
            invoice.BillingPeriod.From == new DateOnly(2026, 3, 1) &&
            invoice.BillingPeriod.To == new DateOnly(2026, 3, 31) &&
            invoice.DueDate == new DateOnly(2026, 3, 5) &&
            invoice.Currency == "CAD"));

        await _invoiceNumberGenerator.Received(1).GenerateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_Lease_StartDate_As_BillingFrom_When_Lease_Starts_Mid_Month()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);
        var lease = CreateLease(
            startDate: new DateOnly(2026, 3, 18),
            endDate: null);

        BillingPeriod? capturedBillingPeriod = null;

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Do<BillingPeriod>(x => capturedBillingPeriod = x),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202603-0002");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedBillingPeriod.Should().NotBeNull();
        capturedBillingPeriod!.From.Should().Be(new DateOnly(2026, 3, 18));
        capturedBillingPeriod.To.Should().Be(new DateOnly(2026, 3, 31));

        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(invoice =>
            invoice.BillingPeriod.From == new DateOnly(2026, 3, 18) &&
            invoice.BillingPeriod.To == new DateOnly(2026, 3, 31)));
    }

    [Fact]
    public async Task Handle_Should_Use_Lease_EndDate_As_BillingTo_When_Lease_Ends_Mid_Month()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);
        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2026, 3, 20));

        BillingPeriod? capturedBillingPeriod = null;

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .ExistsIssuedAsync(
                lease.LeaseId,
                Arg.Do<BillingPeriod>(x => capturedBillingPeriod = x),
                Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202603-0003");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedBillingPeriod.Should().NotBeNull();
        capturedBillingPeriod!.From.Should().Be(new DateOnly(2026, 3, 1));
        capturedBillingPeriod.To.Should().Be(new DateOnly(2026, 3, 20));

        _invoiceRepository.Received(1).Add(Arg.Is<Invoice>(invoice =>
            invoice.BillingPeriod.From == new DateOnly(2026, 3, 1) &&
            invoice.BillingPeriod.To == new DateOnly(2026, 3, 20)));
    }

    [Fact]
    public async Task Handle_Should_Process_Multiple_Leases_With_Mixed_Create_And_Skip()
    {
        // Arrange
        var command = new GenerateInvoicesForBillingPeriodCommand(2026, 3);

        var lease1 = CreateLease(startDate: new DateOnly(2026, 1, 1), endDate: null);
        var lease2 = CreateLease(startDate: new DateOnly(2026, 2, 1), endDate: null);
        var lease3 = CreateLease(startDate: new DateOnly(2026, 3, 10), endDate: null);

        _leaseRepository
            .GetLeasesOverlappingPeriodAsync(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                Arg.Any<CancellationToken>())
            .Returns([lease1, lease2, lease3]);

        _invoiceRepository
            .ExistsIssuedAsync(lease1.LeaseId, Arg.Any<BillingPeriod>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceRepository
            .ExistsIssuedAsync(lease2.LeaseId, Arg.Any<BillingPeriod>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _invoiceRepository
            .ExistsIssuedAsync(lease3.LeaseId, Arg.Any<BillingPeriod>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-1", "INV-2");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(3);

        result.TotalCreated.Should().Be(2);

        result.TotalSkipped.Should().Be(1);

        _invoiceRepository.Received(2).Add(Arg.Any<Invoice>());
        await _invoiceNumberGenerator.Received(2).GenerateAsync(Arg.Any<CancellationToken>());
    }

    private static TenantId NewTenant() => new TenantId(Guid.NewGuid());
    private static UnitId NewUnit() => new UnitId(Guid.NewGuid());


    private static Lease CreateLease(
        DateOnly startDate,
        DateOnly? endDate,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        // Adapt this helper to your real Lease factory signature.
        return Lease.Create(
            occupancy: NewTenant(),
            unitId: NewUnit(),
            monthlyRate: Money.Create(currency, monthlyRentAmount),
            leaseTerm: LeaseTerm.Create(startDate, endDate),
            billingSettings: BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            utilityResponsibility: UtilityResponsibility.Create(
                tenantPaysElectricity: false,
                tenantPaysWater: false));
    }
}