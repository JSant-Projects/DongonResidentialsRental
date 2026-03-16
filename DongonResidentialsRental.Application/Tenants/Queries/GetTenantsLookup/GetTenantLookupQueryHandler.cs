using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantsLookup;

public sealed class GetTenantLookupQueryHandler : IQueryHandler<GetTenantLookupQuery, IReadOnlyList<TenantLookupResponse>>
{
    private readonly IApplicationDBContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public GetTenantLookupQueryHandler(
        IDateTimeProvider dateTimeProvider,
        IApplicationDBContext dbContext)
    {
        _dateTimeProvider = dateTimeProvider;
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<TenantLookupResponse>> Handle(GetTenantLookupQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);
        // Fetch tenants that doesn't have active lease
        var lookup = await _dbContext.Tenants
            .Where(t => 
                !_dbContext.Leases.Any(
                    l => l.Occupancy == t.TenantId && 
                    l.Term.StartDate <= today &&
                    l.Term.EndDate >= today))
            .Select(TenantMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}