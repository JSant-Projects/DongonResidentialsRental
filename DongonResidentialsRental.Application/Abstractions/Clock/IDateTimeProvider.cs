using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateOnly Today { get; }
}
