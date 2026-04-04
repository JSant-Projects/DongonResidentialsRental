using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;
using Unit = DongonResidentialsRental.Domain.Unit.Unit;

namespace DongonResidentialsRental.Application.Leases.Commands.CreateLease;

public sealed class CreateLeaseCommandHandler : ICommandHandler<CreateLeaseCommand, LeaseId>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitRepository _unitRepository;
    private readonly ITenantRepository _tenantRepository;
    public CreateLeaseCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitRepository unitRepository,
        ITenantRepository tenantRepository)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitRepository = unitRepository;
        _tenantRepository = tenantRepository;
    }
    public async Task<LeaseId> Handle(CreateLeaseCommand request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;

        bool unitExists = await _unitRepository.ExistsAsync(request.UnitId, cancellationToken);
        if (!unitExists)
        {
            throw new NotFoundException(nameof(Unit), request.UnitId);
        }

        bool tenantExists = await _tenantRepository.ExistsAsync(request.Occupancy, cancellationToken);
        if (!tenantExists) 
        {
            throw new NotFoundException(nameof(Tenant), request.Occupancy);
        }

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
        var utilityResponsibility = UtilityResponsibility.Create(request.TenantPaysElectricity, request.TenantPaysWater);

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