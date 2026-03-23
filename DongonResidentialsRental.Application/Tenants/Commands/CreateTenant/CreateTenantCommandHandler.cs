using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, TenantId>
{
    private readonly ITenantRepository _tenantRepository;
    public CreateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    public async Task<TenantId> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        bool tenantExistsByEmail = await _tenantRepository
            .ExistsByEmailAsync(
                request.Email, 
                cancellationToken);

        if (tenantExistsByEmail)
        {
            throw new ConflictException($"Tenant with email {request.Email} already exists");
        }
        
        var personalInfo = PersonalInfo.Create(request.FirstName, request.LastName);
        var email = Email.Create(request.Email);
        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        var contactInfo = ContactInfo.Create(email, phoneNumber);

        var tenant = Tenant.Create(personalInfo, contactInfo);

        _tenantRepository.Add(tenant);

        return tenant.TenantId;
    }
}