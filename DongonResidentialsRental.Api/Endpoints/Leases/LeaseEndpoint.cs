using DongonResidentialsRental.Api.Contracts.Leases;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Leases.Commands.ActivateLease;
using DongonResidentialsRental.Application.Leases.Commands.ChangeBillingSettings;
using DongonResidentialsRental.Application.Leases.Commands.ChangeLeaseTerm;
using DongonResidentialsRental.Application.Leases.Commands.ChangeMonthlyRate;
using DongonResidentialsRental.Application.Leases.Commands.ChangeUtilityResponsibility;
using DongonResidentialsRental.Application.Leases.Commands.CreateLease;
using DongonResidentialsRental.Application.Leases.Commands.TerminateLease;
using DongonResidentialsRental.Application.Leases.Queries.GetLeases;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Domain.Unit;

namespace DongonResidentialsRental.Api.Endpoints.Leases;

public static class LeaseEndpoint
{
    public static IEndpointRouteBuilder MapLeaseEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leases")
            .WithTags("Leases");

       
        group.MapPost("/", CreateLease)
            .WithName("CreateLease")
            .WithDescription("Creates a new lease.")
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/", GetLeases)
           .WithName("GetLeases")
           .WithDescription("Retrieves a list of leases.")
           .Produces(StatusCodes.Status200OK);

        group.MapPut("/{leaseId:guid}/activate", ActivateLease)
            .WithName("ActivateLease")
            .WithDescription("Activates a specific lease.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{leaseId:guid}/billing-settings", ChangeBillingSettings)
            .WithName("ChangeBillingSettings")
            .WithDescription("Changes the billing settings for a specific lease.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{leaseId:guid}/lease-term", ChangeLeaseTerm)
            .WithName("ChangeLeaseTerm")
            .WithDescription("Changes the lease term for a specific lease.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{leaseId:guid}/monthly-rate", ChangeMonthlyRate)
            .WithName("ChangeMonthlyRate")
            .WithDescription("Changes the monthly rate for a specific lease.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{leaseId:guid}/utility-responsibility", ChangeUtilityResponsibility)
           .WithName("ChangeUtilityResponsibility")
           .WithDescription("Changes the utility responsibility for a specific lease.")
           .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{leaseId:guid}/terminate", TerminateLease)
            .WithName("TerminateLease")
            .WithDescription("Terminates a specific lease.")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> ActivateLease(
        LeaseId leaseId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateLeaseCommand(leaseId);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeBillingSettings(
        LeaseId leaseId,
        ChangeBillingSettingsRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeBillingSettingsCommand(
            leaseId,
            request.NewDueDayOfMonth,
            request.NewGracePeriodDays);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeLeaseTerm(
        LeaseId leaseId,
        ChangeLeaseTermRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeLeaseTermCommand(
            leaseId,
            request.NewStartDate,
            request.NewEndDate);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeMonthlyRate(
        LeaseId leaseId,
        ChangeMonthlyRateRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeMonthlyRateCommand(
            leaseId,
            request.NewMonthlyRate, 
            request.Currency);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeUtilityResponsibility(
        LeaseId leaseId,
        ChangeUtilityResponsibilityRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUtilityResponsibilityCommand(
            leaseId,
            request.TenantPaysElectricity,
            request.TenantPaysWater);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CreateLease(
        CreateLeaseRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateLeaseCommand(
            new TenantId(request.Occupancy),
            new UnitId(request.UnitId),
            request.StartDate,
            request.EndDate,
            request.MonthlyRate,
            request.DueDayOfMonth, 
            request.GracePeridoDays, 
            request.TenantPaysElectricity,
            request.TenantPaysWater, 
            request.Currency);

        var result = await dispatcher.Send(command, cancellationToken);

        return Results.Created($"/api/leases/{result.Id}", result.Id);
    }

    private static async Task<IResult> TerminateLease(
        LeaseId leaseId,
        TerminateLeaseRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new TerminateLeaseCommand(
            leaseId,
            request.TerminationDate);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetLeases(
        [AsParameters] GetLeasesQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetLeasesQuery(
            queryParams.SearchTerm,
            queryParams.BuildingId,
            queryParams.IsActive,
            queryParams.Page,
            queryParams.PageSize);

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }
}
