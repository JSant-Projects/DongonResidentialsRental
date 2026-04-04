using DongonResidentialsRental.Api.Endpoints.Buildings;
using DongonResidentialsRental.Api.Endpoints.Leases;
using DongonResidentialsRental.Api.Endpoints.Tenants;
using DongonResidentialsRental.Api.Endpoints.Units;

namespace DongonResidentialsRental.Api.Common.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapBuildingEndpoints();
        app.MapTenantEndpoint();
        app.MapUnitEndpoint();
        app.MapLeaseEndpoint();

        return app;
    }
}
