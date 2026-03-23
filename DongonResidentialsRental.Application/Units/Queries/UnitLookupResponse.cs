using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries;

public sealed record UnitLookupResponse(
    Guid UnitId,
    string UnitNumber);
