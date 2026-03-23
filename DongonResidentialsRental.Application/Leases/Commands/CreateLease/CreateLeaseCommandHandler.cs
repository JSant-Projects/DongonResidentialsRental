using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Leases.Commands.CreateLease;

public sealed class CreateLeaseCommandHandler : ICommandHandler<CreateLeaseCommand, LeaseId>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public CreateLeaseCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<LeaseId> Handle(CreateLeaseCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        // Throw exception when unitid from request has active lease
        bool unitHasActiveLease = await _leaseRepository.ExistsActiveLeaseForUnitAsync(
                                    request.UnitId,
                                    today,
                                    cancellationToken);
        if (unitHasActiveLease)
        {
            throw new ConflictException($"Unit ({request.UnitId}) has an active lease. CreateLease Operation is not allowed");
        }

        // Throw exception when tenantid from request has active lease
        bool tenantHasActiveLease = await _leaseRepository.ExistsActiveLeaseForTenantAsync(
                                        request.Occupancy, 
                                        today, 
                                        cancellationToken);
        if (tenantHasActiveLease)
        {
            throw new ConflictException($"Tenant ({request.Occupancy}) has an active lease. CreateLease Operation is not allowed");
        }

        // Term
        var term = LeaseTerm.Create(request.StartDate, request.EndDate);
        // MonthlyRate
        var monthlyRate = Money.Create(request.Currency, request.MonthlyRate);
        // BillingSettings
        var billingSettings = BillingSettings.Create(request.DueDayOfMonth, request.GracePeridoDays);
        // UtilityResponsibility
        var utilityResponsibility = UtilityResponsibility.Create(request.tenantPaysElectricity, request.tenantPaysWater);

        var lease = Lease.Create(
            request.Occupancy, 
            request.UnitId, 
            term, 
            monthlyRate, 
            billingSettings, 
            utilityResponsibility);

        _leaseRepository.Add(lease);

        return lease.LeaseId;
    }
}