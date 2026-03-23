using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Leases.Commands.ActivateLease;

public sealed class ActivateLeaseCommandHandler : ICommandHandler<ActivateLeaseCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    public ActivateLeaseCommandHandler(ILeaseRepository leaseRepository)
    {
        _leaseRepository = leaseRepository;
    }
    public async Task<Unit> Handle(ActivateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null) 
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        lease.Activate();

        return Unit.Value;
    }
}