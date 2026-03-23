using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Lease;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Leases.Queries.GetLeaseById;

public sealed record GetLeaseByIdQuery(LeaseId LeaseId) : IQuery<LeaseResponse>;
