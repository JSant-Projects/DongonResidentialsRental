using DongonResidentialsRental.Domain.Shared;

namespace DongonResidentialsRental.Domain.Meter;

public sealed record MeterReading(DateOnly Date, Decimal Value)
{
    internal static MeterReading Create(DateOnly date, decimal value)
    {
        Ensure.NotNull(date, "Date cannot be null");
        Ensure.NonNegativeDecimal(value, "Meter reading cannot be negative.");
        return new MeterReading(date, value);
    }
}
