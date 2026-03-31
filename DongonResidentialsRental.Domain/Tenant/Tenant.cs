using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Tenant;

public sealed class Tenant
{
    public TenantId TenantId { get; }
    public PersonalInfo PersonalInfo { get; private set; }
    public ContactInfo ContactInfo { get; private set; }

    private Tenant() { }

    private Tenant(TenantId tenantId, PersonalInfo personalInfo, ContactInfo contactInfo)
    {
        TenantId = tenantId;
        PersonalInfo = personalInfo;
        ContactInfo = contactInfo;
    }

    public void ChangeName(PersonalInfo personalInfo)
    {
        Ensure.NotNull(personalInfo, "Personal info cannot be null");
        PersonalInfo = personalInfo;
    }

    public void ChangeContactInfo(ContactInfo contactInfo)
    {
        Ensure.NotNull(contactInfo, "Contact info cannot be null");
        ContactInfo = contactInfo;
    }

    public static Tenant Create(PersonalInfo personalInfo, ContactInfo contactInfo)
    {
        Ensure.NotNull(personalInfo, "Personal info cannot be null");
        Ensure.NotNull(contactInfo, "Contact info cannot be null");

        return new Tenant(
            new TenantId(Guid.NewGuid()),
            personalInfo, 
            contactInfo);
    }
}
