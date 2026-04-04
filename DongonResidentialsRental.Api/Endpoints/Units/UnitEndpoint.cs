using DongonResidentialsRental.Api.Contracts.Units;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Buildings.Queries.GetAvailableBuildingsLookup;
using DongonResidentialsRental.Application.Units.Commands.ActivateUnit;
using DongonResidentialsRental.Application.Units.Commands.CreateUnit;
using DongonResidentialsRental.Application.Units.Commands.DeactivateUnit;
using DongonResidentialsRental.Application.Units.Commands.PutUnitUnderMaintanance;
using DongonResidentialsRental.Application.Units.Queries.GetAvailableUnitsLookupByBuilding;
using DongonResidentialsRental.Application.Units.Queries.GetUnits;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using FluentValidation;

namespace DongonResidentialsRental.Api.Endpoints.Units;

public static class UnitEndpoint
{
    public static IEndpointRouteBuilder MapUnitEndpoint(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/units")
            .WithTags("Units");

        group.MapPost("/", CreateUnit)
            .WithName("CreateUnit")
            .WithDescription("Creates a new unit.")
            .Produces(StatusCodes.Status201Created);

        group.MapPut("/{unitId:guid}/activate", ActivateUnit)
            .WithName("ActivateUnit")
            .WithDescription("Activates a unit.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{unitId:guid}/deactivate", DeactivateUnit)
            .WithName("DeactivateUnit")
            .WithDescription("Deactivates a unit.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{unitId:guid}/maintenance", PutUnitUnderMaintenance)
            .WithName("PutUnitUnderMaintenance")
            .WithDescription("Puts a unit under maintenance.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/", GetUnits)
            .WithName("GetUnits")
            .WithDescription("Retrieves a list of units.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/available", GetAvailableUnitsLookup)
            .WithName("GetAvailableUnitsLookup")
            .WithDescription("Retrieves a list of available units.")
            .Produces(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<IResult> CreateUnit(
        CreateUnitRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var createUnitCommand = new CreateUnitCommand(
            new BuildingId(request.BuildingId),
            request.UnitNumber,
            request.Floor);

        var result = await dispatcher.Send(createUnitCommand, cancellationToken);

        return Results.Created($"/api/units/{result.Id}", result.Id);
    }

    private static async Task<IResult> ActivateUnit(
        UnitId unitId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var activateUnitCommand = new ActivateUnitCommand(unitId);

        var result = await dispatcher.Send(activateUnitCommand, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateUnit(
        UnitId unitId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var deactivateUnitCommand = new DeactivateUnitCommand(unitId);

        var result = await dispatcher.Send(deactivateUnitCommand, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> PutUnitUnderMaintenance(
        UnitId unitId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var putUnitUnderMaintenanceCommand = new PutUnitUnderMaintenanceCommand(unitId);

        var result = await dispatcher.Send(putUnitUnderMaintenanceCommand, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetUnits(
        [AsParameters] GetUnitsQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var buildingId = queryParams.BuildingId.HasValue ? 
                            new BuildingId(queryParams.BuildingId.Value) : 
                            null;

        var getUnitsQuery = new GetUnitsQuery(
            queryParams.Status,
            queryParams.UnitNumber,
            buildingId, 
            queryParams.Floor, 
            queryParams.Page, 
            queryParams.PageSize);

        var result = await dispatcher.Send(getUnitsQuery, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAvailableUnitsLookup(
        [AsParameters] GetAvailableUnitsLookupByBuildingQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (!queryParams.BuildingId.HasValue || queryParams.BuildingId == Guid.Empty)
        {
            return Results.BadRequest("BuildingId is required.");
        }

        var buildingId = new BuildingId(queryParams.BuildingId.Value);

        var getAvailableUnitsLookupQuery = new GetAvailableUnitsLookupByBuildingQuery(buildingId);

        var result = await dispatcher.Send(getAvailableUnitsLookupQuery, cancellationToken);

        return Results.Ok(result);
    }

}
