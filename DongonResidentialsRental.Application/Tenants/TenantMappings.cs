using DongonResidentialsRental.Application.Tenants.Queries;
using DongonResidentialsRental.Domain.Tenant;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants;

public static class TenantMappings
{
    public static Expression<Func<Tenant, TenantResponse>> ToResponse() 
        => tenant => new TenantResponse(
            tenant.TenantId.Id, 
            tenant.PersonalInfo.FirstName,
            tenant.PersonalInfo.LastName,
            tenant.ContactInfo.Email.Value, 
            tenant.ContactInfo.PhoneNumber.Value);

    public static Expression<Func<Tenant, TenantLookupResponse>> ToLookupResponse()
        => tenant => new TenantLookupResponse(
            tenant.TenantId.Id,
            tenant.PersonalInfo.FirstName + " " + tenant.PersonalInfo.LastName);
}
