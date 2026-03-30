using DongonResidentialsRental.Application.Abstractions.Messaging;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TRequest, TResponse> : ICommandHandler<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly ICommandHandler<TRequest, TResponse> _inner;
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public CommandHandler(
            ICommandHandler<TRequest, TResponse> inner,
            IEnumerable<IValidator<TRequest>> validators)
        {
            _inner = inner;
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(x => !x.IsValid)
                    .SelectMany(x => x.Errors)
                    .Where(x => x is not null)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            return await _inner.Handle(request, cancellationToken);
        }
    }

    internal sealed class QueryHandler<TRequest, TResponse> : IQueryHandler<TRequest, TResponse>
        where TRequest : IQuery<TResponse>
    {
        private readonly IQueryHandler<TRequest, TResponse> _inner;
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public QueryHandler(
            IQueryHandler<TRequest, TResponse> inner,
            IEnumerable<IValidator<TRequest>> validators)
        {
            _inner = inner;
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(x => !x.IsValid)
                    .SelectMany(x => x.Errors)
                    .Where(x => x is not null)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            return await _inner.Handle(request, cancellationToken);
        }
    }
}
