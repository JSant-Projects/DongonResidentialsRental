using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries;

public sealed record UnitResponse(
    Guid UnitId, 
    string UnitNumber, 
    int? Floor);
