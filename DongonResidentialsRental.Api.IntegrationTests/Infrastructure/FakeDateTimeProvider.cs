using DongonResidentialsRental.Application.Abstractions.Clock;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateOnly Today { get; set; } = new DateOnly(2026, 1, 1);
}
