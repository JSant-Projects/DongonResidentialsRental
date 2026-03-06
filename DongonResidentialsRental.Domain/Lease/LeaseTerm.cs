using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Lease;

public sealed record LeaseTerm(DateOnly StartDate, DateOnly? EndDate)
{
    public static LeaseTerm Create(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < startDate)
            throw new DomainException("End date cannot be before start date");
        return new LeaseTerm(startDate, endDate);
    }

    public bool Overlaps(LeaseTerm other)
    {
        var thisEnd = EndDate ?? DateOnly.MaxValue;
        var otherEnd = other.EndDate ?? DateOnly.MaxValue;

        return StartDate <= otherEnd &&
               other.StartDate <= thisEnd;
    }

    public bool Includes(DateOnly date)
        => date >= StartDate &&
           (EndDate is null || date <= EndDate.Value);

}
