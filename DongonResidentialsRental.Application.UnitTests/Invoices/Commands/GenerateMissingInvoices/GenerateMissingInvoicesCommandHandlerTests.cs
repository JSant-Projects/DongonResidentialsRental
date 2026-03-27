using AwesomeAssertions;
using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Invoices.Commands.GenerateMissingInvoices;
using DongonResidentialsRental.Application.Invoices.Services;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using NSubstitute;
using Xunit;

namespace DongonResidentialsRental.Application.UnitTests.Invoices.Commands.GenerateMissingInvoices;

public sealed class GenerateMissingInvoicesCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly ILeaseRepository _leaseRepository = Substitute.For<ILeaseRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator = Substitute.For<IInvoiceNumberGenerator>();

    private readonly GenerateMissingInvoicesCommandHandler _handler;

    public GenerateMissingInvoicesCommandHandlerTests()
    {
        _handler = new GenerateMissingInvoicesCommandHandler(
            _invoiceRepository,
            _leaseRepository,
            _dateTimeProvider,
            _invoiceNumberGenerator);
    }

    [Fact]
    public async Task Handle_Should_Return_Zeroes_When_No_Active_Leases_Exist()
    {
        // Arrange
        var today = new DateOnly(2026, 3, 26);
        var command = new GenerateMissingInvoicesCommand(today);

        _leaseRepository
            .GetActiveLeases(today, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(0);
        result.TotalCreated.Should().Be(0);

        await _invoiceRepository.DidNotReceive()
            .GetLatestBillingPeriodsByLeaseIdsAsync(
                Arg.Any<IReadOnlyCollection<LeaseId>>(),
                Arg.Any<CancellationToken>());

        await _invoiceNumberGenerator.DidNotReceive()
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.DidNotReceive()
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Create_Invoices_For_All_Missing_Periods_When_Lease_Has_No_Previous_Invoice()
    {
        // Arrange
        var today = new DateOnly(2026, 3, 26);
        var command = new GenerateMissingInvoicesCommand(today);

        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 15),
            endDate: null,
            monthlyRentAmount: 1200m,
            currency: "CAD",
            dueDayOfMonth: 5);

        _leaseRepository
            .GetActiveLeases(today, Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .GetLatestBillingPeriodsByLeaseIdsAsync(
                Arg.Is<IReadOnlyCollection<LeaseId>>(x => x.Count == 1 && x.Contains(lease.LeaseId)),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeaseId, BillingPeriod>());

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-0001", "INV-0002");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(1);
        result.TotalCreated.Should().Be(2);

        await _invoiceNumberGenerator.Received(2)
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.Received(2)
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Create_Invoices_Only_For_Missing_Periods_After_Latest_Billing_Period()
    {
        // Arrange
        var today = new DateOnly(2026, 4, 10);
        var command = new GenerateMissingInvoicesCommand(today);

        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null,
            monthlyRentAmount: 1500m,
            currency: "CAD",
            dueDayOfMonth: 3);

        var latestBillingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        _leaseRepository
            .GetActiveLeases(today, Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .GetLatestBillingPeriodsByLeaseIdsAsync(
                Arg.Any<IReadOnlyCollection<LeaseId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeaseId, BillingPeriod>
            {
                [lease.LeaseId] = latestBillingPeriod
            });

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-0002", "INV-0003");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(1);
        result.TotalCreated.Should().Be(2);

        _invoiceRepository.Received(2)
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Not_Create_Invoice_When_No_Missing_Periods_Exist()
    {
        // Arrange
        var today = new DateOnly(2026, 3, 5);
        var command = new GenerateMissingInvoicesCommand(today);

        var lease = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        var latestBillingPeriod = BillingPeriod.Create(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28));

        _leaseRepository
            .GetActiveLeases(today, Arg.Any<CancellationToken>())
            .Returns([lease]);

        _invoiceRepository
            .GetLatestBillingPeriodsByLeaseIdsAsync(
                Arg.Any<IReadOnlyCollection<LeaseId>>(),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeaseId, BillingPeriod>
            {
                [lease.LeaseId] = latestBillingPeriod
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(1);
        result.TotalCreated.Should().Be(0);

        await _invoiceNumberGenerator.DidNotReceive()
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.DidNotReceive()
            .Add(Arg.Any<Invoice>());
    }

    [Fact]
    public async Task Handle_Should_Process_Multiple_Leases_With_Different_Missing_Periods()
    {
        // Arrange
        var today = new DateOnly(2026, 4, 15);
        var command = new GenerateMissingInvoicesCommand(today);

        var lease1 = CreateLease(
            startDate: new DateOnly(2026, 1, 1),
            endDate: null);

        var lease2 = CreateLease(
            startDate: new DateOnly(2026, 2, 1),
            endDate: null);

        var lease3 = CreateLease(
            startDate: new DateOnly(2026, 4, 1),
            endDate: null);

        _leaseRepository
            .GetActiveLeases(today, Arg.Any<CancellationToken>())
            .Returns([lease1, lease2, lease3]);

        _invoiceRepository
            .GetLatestBillingPeriodsByLeaseIdsAsync(
                Arg.Is<IReadOnlyCollection<LeaseId>>(x =>
                    x.Count == 3 &&
                    x.Contains(lease1.LeaseId) &&
                    x.Contains(lease2.LeaseId) &&
                    x.Contains(lease3.LeaseId)),
                Arg.Any<CancellationToken>())
            .Returns(new Dictionary<LeaseId, BillingPeriod>
            {
                [lease1.LeaseId] = BillingPeriod.Create(
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 1, 31)),

                [lease2.LeaseId] = BillingPeriod.Create(
                    new DateOnly(2026, 3, 1),
                    new DateOnly(2026, 3, 31))
            });

        _invoiceNumberGenerator
            .GenerateAsync(Arg.Any<CancellationToken>())
            .Returns("INV-1001", "INV-1002");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalEvaluated.Should().Be(3);
        result.TotalCreated.Should().Be(2);

        await _invoiceNumberGenerator.Received(2)
            .GenerateAsync(Arg.Any<CancellationToken>());

        _invoiceRepository.Received(2)
            .Add(Arg.Any<Invoice>());
    }


    private static TenantId NewTenant() => new TenantId(Guid.NewGuid());
    private static UnitId NewUnit() => new UnitId(Guid.NewGuid());

    private static Lease CreateLease(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        decimal monthlyRentAmount = 1200m,
        string currency = "CAD",
        int dueDayOfMonth = 1,
        int gracePeriodDays = 0)
    {
        return Lease.Create(
            NewTenant(),
            NewUnit(),
            LeaseTerm.Create(startDate ?? new DateOnly(2026, 1, 1), endDate),
            Money.Create(currency, monthlyRentAmount),
            BillingSettings.Create(dueDayOfMonth, gracePeriodDays),
            UtilityResponsibility.Create(false, false));
    }
}