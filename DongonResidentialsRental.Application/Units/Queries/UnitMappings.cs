using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries;

public static class UnitMappings
{
    public static Expression<Func<Unit, UnitResponse>> ToResponse() =>
        unit => new UnitResponse(
            unit.UnitId.Id,
            unit.UnitNumber,
            unit.Floor);

    public static Expression<Func<Unit, UnitLookupResponse>> ToLookupResponse() =>
       unit => new UnitLookupResponse(
           unit.UnitId.Id,
           unit.UnitNumber);
}
