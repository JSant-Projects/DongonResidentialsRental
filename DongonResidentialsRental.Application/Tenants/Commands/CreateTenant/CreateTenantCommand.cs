using DongonResidentialsRental.Application.Abstractions.Messaging;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string FirstName, 
    string LastName,
    string Email,
    string PhoneNumber) : ICommand<TenantId>;
