using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeBillingSettings;

public sealed class ChangeBillingSettingsCommandHandler : ICommandHandler<ChangeBillingSettingsCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ChangeBillingSettingsCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(ChangeBillingSettingsCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null) 
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var newBillingSettings = BillingSettings.Create(request.NewDueDayOfMonth, request.NewGracePeriodDays);
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);

        lease.ChangeBillingSettings(newBillingSettings, today);

        return Unit.Value;
    }
}
