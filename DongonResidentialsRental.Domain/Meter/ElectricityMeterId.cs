using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Meter;

public sealed record ElectricityMeterId(Guid Id)
{
    public static bool TryParse(string? value, out ElectricityMeterId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new ElectricityMeterId(guid);
            return true;
        }

        result = null;
        return false;
    }

    public override string ToString() => Id.ToString();
}
