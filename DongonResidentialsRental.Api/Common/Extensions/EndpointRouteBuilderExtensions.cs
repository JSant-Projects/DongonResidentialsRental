using DongonResidentialsRental.Api.Endpoints.Buildings;
using DongonResidentialsRental.Api.Endpoints.Tenants;

namespace DongonResidentialsRental.Api.Common.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapBuildingEndpoints();
        app.MapTenantEndpoint();

        return app;
    }
}
