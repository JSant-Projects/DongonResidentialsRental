using DongonResidentialsRental.Application.Exceptions;
using DongonResidentialsRental.Domain.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DongonResidentialsRental.Api.Common.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            traceId);

        object problem = exception switch
        {
            ValidationException validationException => new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Title = "Validation failed.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path
            },

            OperationNotAllowedException operationNotAllowedException => 
                    new ProblemDetails
            {
                Title = "Operation not allowed.",
                Status = StatusCodes.Status409Conflict,
                Detail = operationNotAllowedException.Message,
                Instance = httpContext.Request.Path
            },

            DomainException domainException => new ProblemDetails
            {
                Title = "Business rule violation.",
                Status = StatusCodes.Status400BadRequest,
                Detail = domainException.Message,
                Instance = httpContext.Request.Path
            },

            NotFoundException notFoundException => new ProblemDetails
            {
                Title = "Resource not found.",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundException.Message,
                Instance = httpContext.Request.Path
            },

            ConflictException conflictException => new ProblemDetails
            {
                Title = "Conflict.",
                Status = StatusCodes.Status409Conflict,
                Detail = conflictException.Message,
                Instance = httpContext.Request.Path
            },

            JsonException jsonException => new ProblemDetails
            {
                Title = "Invalid JSON.",
                Status = StatusCodes.Status400BadRequest,
                Detail = jsonException.Message,
                Instance = httpContext.Request.Path
            },

            BadHttpRequestException badRequestException
                when badRequestException.InnerException is JsonException jsonException
                => new ProblemDetails
                {
                    Title = "Invalid JSON.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = jsonException.Message,
                    Instance = httpContext.Request.Path
                },

            BadHttpRequestException badRequestException => new ProblemDetails 
            {
                Title = "Invalid Request.",
                Status = StatusCodes.Status400BadRequest,
                Detail = badRequestException.Message,
                Instance = httpContext.Request.Path
            },

            _ => new ProblemDetails
            {
                Title = "Server error.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred.",
                Instance = httpContext.Request.Path
            }
        };

        if (problem is ProblemDetails pd)
        {
            pd.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = pd.Status ?? StatusCodes.Status500InternalServerError;
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
