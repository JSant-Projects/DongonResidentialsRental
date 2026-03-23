using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Lease;

namespace DongonResidentialsRental.Application.Leases.Commands.ChangeUtilityResponsibility;

public sealed class ChangeUtilityResponsibilityCommandHandler : ICommandHandler<ChangeUtilityResponsibilityCommand, Unit>
{
    private readonly ILeaseRepository _leaseRepository;
    public ChangeUtilityResponsibilityCommandHandler(ILeaseRepository leaseRepository)
    {
        _leaseRepository = leaseRepository;
    }
    public async Task<Unit> Handle(ChangeUtilityResponsibilityCommand request, CancellationToken cancellationToken)
    {
        var lease = await _leaseRepository.GetByIdAsync(request.LeaseId, cancellationToken);

        if (lease is null)
        {
            throw new NotFoundException(nameof(Lease), request.LeaseId);
        }

        var utilityResponsibility = UtilityResponsibility.Create(request.TenantPaysElectricity, request.TenantPaysWater);

        lease.ChangeUtilityResponsibility(utilityResponsibility);

        return Unit.Value;
    }
}