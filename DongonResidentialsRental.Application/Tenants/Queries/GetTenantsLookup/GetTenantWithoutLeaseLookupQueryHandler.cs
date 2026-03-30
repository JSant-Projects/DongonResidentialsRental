using DongonResidentialsRental.Application.Abstractions.Clock;
using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DongonResidentialsRental.Application.Tenants.Queries.GetTenantsLookup;

public sealed class GetTenantWithoutLeaseLookupQueryHandler : IQueryHandler<GetTenantWithoutLeaseLookupQuery, IReadOnlyList<TenantLookupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public GetTenantWithoutLeaseLookupQueryHandler(
        IDateTimeProvider dateTimeProvider,
        IApplicationDbContext dbContext)
    {
        _dateTimeProvider = dateTimeProvider;
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<TenantLookupResponse>> Handle(GetTenantWithoutLeaseLookupQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Today);
        // Fetch tenants that doesn't have active lease
        var lookup = await _dbContext.Tenants
            .WithoutActiveLease(_dbContext.Leases, today)
            .Select(TenantMappings.ToLookupResponse())
            .ToListAsync(cancellationToken);

        return lookup;
    }
}