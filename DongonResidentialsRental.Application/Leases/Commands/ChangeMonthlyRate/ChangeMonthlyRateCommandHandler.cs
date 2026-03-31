using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeMonthlyRate;

public sealed class ChangeMonthlyRateCommandHandler : ICommandHandler<ChangeMonthlyRateCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ChangeMonthlyRateCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(ChangeMonthlyRateCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var newMonthlyRate = Money.Create(request.Currency, request.NewMonthlyRate);
        var today = _dateTimeProvider.Today;
        
        lease.ChangeMonthlyRate(newMonthlyRate, today);

        return Unit.Value;
    }
}
