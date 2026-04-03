using DongonResidentialsRental.Api.Contracts.Buildings;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Buildings.Commands.CreateBuilding;
using DongonResidentialsRental.Application.Buildings.Queries.GetAvailableBuildingsLookup;
using DongonResidentialsRental.Application.Buildings.Queries.GetBuildingById;
using DongonResidentialsRental.Application.Buildings.Queries.GetBuildings;

namespace DongonResidentialsRental.Api.Endpoints.Buildings;

public static class BuildingEndpoint
{
    public static IEndpointRouteBuilder MapBuildingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/buildings")
            .WithTags("Buildings");

        group.MapPost("/", CreateBuilding)
            .WithName("CreateBuilding")
            .WithDescription("Creates a new building.")
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/", GetBuildings)
            .WithName("GetBuildings")
            .WithDescription("Retrieves a list of buildings.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/available", GetAvailableBuildingsLookup)
            .WithName("GetAvailableBuildingsLookup")
            .WithDescription("Retrieves a list of available buildings for lookup.")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<IResult> CreateBuilding(
        CreateBuildingRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {

        var command = new CreateBuildingCommand(
            request.Name,
            request.AddressStreet,
            request.AddressCity,
            request.AddressProvince,
            request.AddressPostalCode);

        var result = await dispatcher.Send(command, cancellationToken);

        return Results.Created($"/api/buildings/{result.Id}", new
        {
            Id = result.Id,
        });
    }

    private static async Task<IResult> GetBuildings(
        [AsParameters] GetBuildingsQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetBuildingsQuery(
            queryParams.Status,
            queryParams.SearchTerm,
            queryParams.Page,
            queryParams.PageSize);

        var result = await dispatcher.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAvailableBuildingsLookup(
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableBuildingsLookupQuery();

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }
}
