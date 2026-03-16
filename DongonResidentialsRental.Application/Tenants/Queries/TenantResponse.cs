using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries;

public sealed record TenantResponse(
    Guid TenantId, 
    string FirstName,
    string LastName,
    string Email, 
    string PhoneNumber);
