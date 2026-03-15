using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantById;

public sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    private readonly IApplicationDBContext _dbContext;
    public GetTenantByIdQueryHandler(IApplicationDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<TenantResponse> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext
            .Tenants.AsNoTracking()
            .Where(t => t.TenantId == request.TenantId)
            .Select(TenantMappings.ToResponse())
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        return tenant;
    }
}