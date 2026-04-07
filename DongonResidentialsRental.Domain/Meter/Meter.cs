using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace DongonResidentialsRental.Domain.Meter;

public sealed class Meter
{
    public MeterId MeterId { get; }
    public UnitId UnitId { get; }
    public MeterType Type { get; }
    public MeterStatus Status { get; private set; }

    private readonly List<MeterReading> _readings = new();
    public IReadOnlyList<MeterReading> Readings => _readings.AsReadOnly();

    private Meter() { }

    private Meter(
        MeterId meterId,
        UnitId unitId, 
        MeterType type)
    {
        MeterId = meterId;
        UnitId = unitId;
        Type = type;
        Status = MeterStatus.Active;
    }

    public static Meter Create(UnitId unitId, MeterType type)
    {
        Ensure.NotNull(unitId, "Unit ID cannot be null");

        return new Meter(
            new MeterId(Guid.NewGuid()),
            unitId, 
            type);
    }

    public void Deactivate()
    {
        EnsureActive();
        Status = MeterStatus.Inactive;
    }

    public void AddReading(DateOnly date, decimal value)
    {
        EnsureActive();
        EnsureReadingIsValid(date, value);
        var meterReading = MeterReading.Create(date, value);
        _readings.Add(meterReading);
    }

    private void EnsureActive()
    {
        if (Status == MeterStatus.Active)
            return;

        throw new OperationNotAllowedException("Meter is not active.");
    }


    private void EnsureReadingIsValid(DateOnly date, decimal value)
    {
        var last = _readings.OrderBy(r => r.Date).LastOrDefault();

        if (last is not null)
        {
            if (date <= last.Date)
                throw new DomainException("Reading date must be later than last reading.");

            if (value < last.Value)
                throw new DomainException("Reading value cannot be less than last reading.");
        }
    }
}
