using DongonResidentialsRental.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static DongonResidentialsRental.Application.Behaviors.LoggingDecorator;

namespace DongonResidentialsRental.Application.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class QueryHandler<TRequest, TResponse> : 
        IQueryHandler<TRequest, TResponse>
        where TRequest : IQuery<TResponse>
    {
        private readonly IQueryHandler<TRequest, TResponse> _inner;
        private readonly ILogger<QueryHandler<TRequest, TResponse>> _logger;
        public QueryHandler(
            IQueryHandler<TRequest, TResponse> inner,
            ILogger<QueryHandler<TRequest, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var queryName = typeof(TRequest).Name;

            // Log the incoming query
            _logger.LogInformation(
                "Handling query: {QueryName}", 
                queryName);

            // Start the stopwatch to measure execution time
            var start = Stopwatch.GetTimestamp();

            try
            {

                var response = await _inner.Handle(request, cancellationToken);

                // Stop the stopwatch and calculate elapsed time
                var elapsedTime = Stopwatch.GetElapsedTime(start);

                // Log the outgoing response
                _logger.LogInformation(
                    "Handled query: {QueryName} in {ElapsedMilliseconds:F2}ms",
                    queryName,
                    elapsedTime.TotalMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                var elapsedTime = Stopwatch.GetElapsedTime(start);

                _logger.LogError(
                    ex,
                    "Error executing query: {QueryName} after {ElapsedMilliseconds:F2}ms",
                    queryName,
                    elapsedTime.TotalMilliseconds);

                throw;
            }
            
        }
    }

    internal sealed class CommandHandler<TRequest, TResponse> : 
        ICommandHandler<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly ICommandHandler<TRequest, TResponse> _inner;
        private readonly ILogger<CommandHandler<TRequest, TResponse>> _logger;

        public CommandHandler(
            ICommandHandler<TRequest, TResponse> inner,
            ILogger<CommandHandler<TRequest, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var commandName = typeof(TRequest).Name;

            _logger.LogInformation("Executing command: {CommandName}", commandName);

            var start = Stopwatch.GetTimestamp();

            try
            {
                var response = await _inner.Handle(request, cancellationToken);

                var elapsedTime = Stopwatch.GetElapsedTime(start);

                _logger.LogInformation(
                    "Executed command: {CommandName} in {ElapsedMilliseconds:F2}ms",
                    commandName,
                    elapsedTime.TotalMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                var elapsedTime = Stopwatch.GetElapsedTime(start);

                _logger.LogError(
                    ex,
                    "Error executing command: {CommandName} after {ElapsedMilliseconds:F2}ms",
                    commandName,
                    elapsedTime.TotalMilliseconds);

                throw;
            }
        }
    }
}
