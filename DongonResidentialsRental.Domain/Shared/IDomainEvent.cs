using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
