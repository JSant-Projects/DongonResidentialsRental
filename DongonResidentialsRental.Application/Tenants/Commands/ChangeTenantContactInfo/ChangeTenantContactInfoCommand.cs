using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Commands.ChangeTenantContactInfo;

public sealed record ChangeTenantContactInfoCommand(
    TenantId TenantId,
    string Email,
    string PhoneNumber) : ICommand<Unit>;
