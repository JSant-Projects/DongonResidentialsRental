using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Leases.Commands.TerminateLease;

public sealed class TerminateLeaseCommandHandler : ICommandHandler<TerminateLeaseCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public TerminateLeaseCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(TerminateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var today = _dateTimeProvider.Today;

        lease.Terminate(request.TerminationDate, today);

        return Unit.Value;
    }
}
