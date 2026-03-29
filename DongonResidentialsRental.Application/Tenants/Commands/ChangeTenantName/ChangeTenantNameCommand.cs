using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantName;

public sealed record ChangeTenantNameCommand(
    TenantId TenantId, 
    string FirstName, 
    string LastName) : ICommand<Unit>;
