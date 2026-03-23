using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Unit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnitById;

public sealed record GetUnitByIdQuery(UnitId UnitId) : IQuery<UnitResponse>;
