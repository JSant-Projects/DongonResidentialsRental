using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Extensions;
using DongonResidentialsRental.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenants;

public sealed class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, PagedResult<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public GetTenantsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PagedResult<TenantResponse>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Tenants
            .AsNoTracking()
            .ApplySearch(request.SearchTerm);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyOrdering()
            .ApplyPaging(request.Page, request.PageSize)
            .Select(TenantMappings.ToResponse())
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantResponse>(
            items, 
            request.Page, 
            request.PageSize, 
            totalCount);
    }
}