using DongonResidentialsRental.Api.Contracts.Invoices;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Application.Invoices.Commands.AddInvoiceLine;
using DongonResidentialsRental.Application.Invoices.Commands.CancelInvoice;
using DongonResidentialsRental.Application.Invoices.Commands.CreateInvoice;
using DongonResidentialsRental.Application.Invoices.Commands.IssueInvoice;
using DongonResidentialsRental.Application.Invoices.Queries.GetInvoiceDetails;
using DongonResidentialsRental.Application.Invoices.Queries.GetInvoices;
using DongonResidentialsRental.Application.Invoices.Queries.GetInvoicesDueSoon;
using DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoiceForPayment;
using DongonResidentialsRental.Application.Invoices.Queries.GetOutstandingInvoices;
using DongonResidentialsRental.Application.Models;
using DongonResidentialsRental.Domain.Invoice;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;

namespace DongonResidentialsRental.Api.Endpoints.Invoices;

public static class InvoiceEndpoint
{
    public static IEndpointRouteBuilder MapInvoiceEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices");


        group.MapPost("/", CreateInvoice)
                    .WithName("CreateInvoice")
                    .WithDescription("Creates a new invoice for a specific lease and period.")
                    .Produces(StatusCodes.Status201Created);

        group.MapGet("/", GetInvoices)
            .WithName("GetInvoices")
            .WithDescription("Retrieves a list of invoices with optional filtering by lease, period, and search term.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/{invoiceId:guid}", GetinvoiceDetails)
            .WithName("GetInvoiceDetails")
            .WithDescription("Retrieves detailed information about a specific invoice, including line items and allocations.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/due-soon", GetInvoicesDueSoon)
            .WithName("GetInvoicesDueSoon")
            .WithDescription("Retrieves a list of invoices that are due within a specified number of days.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/outstanding-for-payment", GetOutstandingInvoicesForPayment)
            .WithName("GetOutstandingInvoicesForPayment")
            .WithDescription("Retrieves a list of outstanding invoices for a specific lease that are eligible for payment.")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/outstanding", GetOutstandingInvoices)
            .WithName("GetOutstandingInvoices")
            .WithDescription("Retrieves a list of outstanding invoices with optional filtering by tenant and lease.")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/{invoiceId:guid}/lines", AddInvoiceLine)
            .WithName("AddInvoiceLine")
            .WithDescription("Adds a new line item to a specific invoice.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{invoiceId:guid}/cancel", CancelInvoice)
            .WithName("CancelInvoice")
            .WithDescription("Cancels a specific invoice.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/{invoiceId:guid}/issue", IssueInvoice)
            .WithName("IssueInvoice")
            .WithDescription("Issues a specific invoice.")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> GetInvoices(
        [AsParameters] GetInvoicesQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {

        var leaseId = queryParams.LeaseId.HasValue ? new LeaseId(queryParams.LeaseId.Value) : null;
        var period = queryParams.From.HasValue && queryParams.To.HasValue
            ? new DateRange(queryParams.From.Value, queryParams.To.Value)
            : null;

        var query = new GetInvoicesQuery(
            leaseId,
            period,
            queryParams.SearchTerm,
            queryParams.Page,
            queryParams.PageSize);

        var result = await dispatcher.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetinvoiceDetails(
        Guid invoiceId,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoiceDetailsQuery(new InvoiceId(invoiceId));

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetInvoicesDueSoon(
        [AsParameters] GetInvoicesDueSoonQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoicesDueSoonQuery(
            queryParams.Days,
            queryParams.Page,
            queryParams.PageSize);

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetOutstandingInvoicesForPayment(
        [AsParameters] GetOutstandingInvoicesForPaymentQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetOutstandingInvoicesForPaymentQuery(
                            new LeaseId(queryParams.LeaseId));

        var result = await dispatcher.Send(query, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetOutstandingInvoices(
        [AsParameters] GetOutstandingInvoicesQueryParams queryParams,
        IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var leaseId = queryParams.LeaseId.HasValue ? new LeaseId(queryParams.LeaseId.Value) : null;
        var tenantId = queryParams.TenantId.HasValue ? new TenantId(queryParams.TenantId.Value) : null;

        var query = new GetOutstandingInvoicesQuery(
            tenantId,
            leaseId,
            queryParams.Page,
            queryParams.PageSize);
        var result = await dispatcher.Send(query, cancellationToken);
        return Results.Ok(result);
    }


    private static async Task<IResult> CreateInvoice(
        CreateInvoiceRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var period = new DateRange(request.From, request.To);

        var command = new CreateInvoiceCommand(
            new LeaseId(request.LeaseId),
            period);

        var result = await dispatcher.Send(command, cancellationToken);

        return Results.Created($"/api/invoices/{result.Id}", result.Id);
    }

    private static async Task<IResult> AddInvoiceLine(
        Guid invoiceId,
        AddInvoiceLineRequest request,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new AddInvoiceLineCommand(
            new InvoiceId(invoiceId),
            request.Description,
            request.Quantity, 
            request.Price, 
            request.LineType);

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CancelInvoice(
        Guid invoiceId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CancelInvoiceCommand(
            new InvoiceId(invoiceId));

        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> IssueInvoice(
        Guid invoiceId,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var command = new IssueInvoiceCommand(
            new InvoiceId(invoiceId));
        
        await dispatcher.Send(command, cancellationToken);

        return Results.NoContent();
    }
}
