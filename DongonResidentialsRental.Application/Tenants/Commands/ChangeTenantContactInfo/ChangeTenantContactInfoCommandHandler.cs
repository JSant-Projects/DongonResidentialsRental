using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantContactInfo;

public sealed class ChangeTenantContactInfoCommandHandler : ICommandHandler<ChangeTenantContactInfoCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;
    public ChangeTenantContactInfoCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    public async Task<Unit> Handle(ChangeTenantContactInfoCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        var newEmail = Email.Create(request.Email);
        var newPhoneNumber = PhoneNumber.Create(request.PhoneNumber);
        var newContactInfo = ContactInfo.Create(newEmail, newPhoneNumber);
        tenant.ChangeContactInfo(newContactInfo);

        return Unit.Value;
    }
}
