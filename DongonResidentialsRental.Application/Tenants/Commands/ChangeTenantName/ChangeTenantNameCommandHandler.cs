using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantName;

public sealed class ChangeTenantNameCommandHandler : ICommandHandler<ChangeTenantNameCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;
    public ChangeTenantNameCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    public async Task<Unit> Handle(ChangeTenantNameCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        var personalInfo = PersonalInfo.Create(request.FirstName, request.LastName);
        tenant.ChangeName(personalInfo);

        return Unit.Value;
    }
}
