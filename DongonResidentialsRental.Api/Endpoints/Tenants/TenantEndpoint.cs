using DongonResidentialsRental.Api.Contracts.Tenants;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantContactInfo;
using DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantName;
using DongonResidentialsRental.Application.Tenants.Commands.CreateTenant;
using DongonResidentialsRental.Application.Tenants.Queries.GetTenants;
using DongonResidentialsRental.Application.Tenants.Queries.GetTenantsLookup;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Endpoints.Tenants;

public static class TenantEndpoint
{
    public static IEndpointRouteBuilder MapTenantEndpoint(this IEndpointRouteBuilder builder) 
    {
        var group = builder.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapPost("/", CreateTenant)
            .WithName("CreateTenant")
            .WithDescription("Creates a new tenant.")
            .Produces(StatusCodes.Status201Created);

        group.MapPost("/{tenantId:guid}/change-tenant-contact", 
            ChangeTenantContactInfo)
            .WithName("ChangeTenantContactInfo")
            .WithDescription("Changes the contact information of a tenant.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{tenantId:guid}/change-tenant-name", 
            ChangeTenantName)
            .WithName("ChangeTenantName")
            .WithDescription("Changes the name of a tenant.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/", GetTenants)
            .WithName("GetTenants")
            .WithDescription("Retrieves a list of tenants.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/without-lease", GetTenantsWithoutLeaseLookup)
            .WithName("GetTenantsWithoutLeaseLookup")
            .WithDescription("Retrieves a list of tenants without active lease for lookup.")
            .Produces(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<IResult> CreateTenant(
        CreateTenantRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var createTenantCommand = new CreateTenantCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var result = await dispatcher.Send(createTenantCommand, cancellationToken);

        return Results.Created($"/api/tenants/{result.Id}", result.Id);
    }

    private static async Task<IResult> ChangeTenantContactInfo(
        TenantId tenantId,
        ChangeTenantContactInfoRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTenantContactInfoCommand(
            tenantId,
            request.Email,
            request.PhoneNumber);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeTenantName(
        TenantId tenantId,
        ChangeTenantNameRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTenantNameCommand(
            tenantId,
            request.FirstName,
            request.LastName);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetTenants(
        [AsParameters] GetTenantsQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantsQuery(
            queryParams.SearchTerm,
            queryParams.Page,
            queryParams.PageSize);

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetTenantsWithoutLeaseLookup(
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantWithoutLeaseLookupQuery();

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }
}
