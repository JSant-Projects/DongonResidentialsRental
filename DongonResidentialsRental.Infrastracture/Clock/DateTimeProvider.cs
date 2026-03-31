using DongonResidentialsRental.Application.Abstractions.Clock;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Infrastracture.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
