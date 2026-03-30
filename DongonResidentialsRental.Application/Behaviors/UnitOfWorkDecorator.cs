using DongonResidentialsRental.Application.Abstractions.Data;
using DongonResidentialsRental.Application.Abstractions.Messaging;
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

        public CommandHandler(
            ICommandHandler<TRequest, TResponse> inner,
            IApplicationDbContext dbContext)
        {
            _inner = inner;
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var response = await _inner.Handle(request, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return response;
        }
    }
}
