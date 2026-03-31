using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeLeaseTerm;

public sealed class ChangeLeaseTermCommandHandler : ICommandHandler<ChangeLeaseTermCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    public ChangeLeaseTermCommandHandler(
        ILeaseRepository leaseRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _leaseRepository = leaseRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    public async Task<Unit> Handle(ChangeLeaseTermCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var newTerm = LeaseTerm.Create(request.NewStartDate, request.NewEndDate);
        var today = _dateTimeProvider.Today;

        lease.ChangeLeaseTerm(newTerm, today);

        return Unit.Value;
    }
}
