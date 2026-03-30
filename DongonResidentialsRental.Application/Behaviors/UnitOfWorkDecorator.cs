using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Behaviors;

internal static class UnitOfWorkDecorator
{
    internal sealed class CommandHandler<TRequest, TResponse> : ICommandHandler<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        private readonly ICommandHandler<TRequest, TResponse> _inner;
        private readonly IApplicationDbContext _dbContext;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public CommandHandler(
            ICommandHandler<TRequest, TResponse> inner,
            IApplicationDbContext dbContext,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _inner = inner;
            _dbContext = dbContext;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var response = await _inner.Handle(request, cancellationToken);

            IReadOnlyCollection<IDomainEvent> domainEvents = _dbContext.GetDomainEvents();

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.ClearDomainEvents();

            await _domainEventDispatcher.Publish(domainEvents, cancellationToken);

            return response;
        }
    }
}
