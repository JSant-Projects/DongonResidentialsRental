using Bogus;
using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Tenant;
using DongonResidentialsRental.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DongonResidentialsRental.Api.IntegrationTests.Infrastructure;

public static class TenantSeederHelper
{
    public static async Task SeedTenantsAsync(
        IntegrationTestWebAppFactory factory,
        params Tenant[] tenants)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Tenants.AddRangeAsync(tenants);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<Tenant> SeedTenantAsync(
        IntegrationTestWebAppFactory factory,
        string tenantFirstName = "John",
        string tenantLastName = "Doe",
        string tenantEmail = "john.doe@email.com",
        string tenantPhoneNumber = "09123456789")
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenant = CreateTenant(
            tenantFirstName,
            tenantLastName,
            tenantEmail,
            tenantPhoneNumber);

        await dbContext.Tenants.AddAsync(tenant);
        await dbContext.SaveChangesAsync();

        return tenant;
    }

    public static Tenant CreateTenant(
       string tenantFirstName,
       string tenantLastName,
       string tenantEmail,
       string tenantPhoneNumber)
    {

        var email = Email.Create(tenantEmail);
        var phoneNumber = PhoneNumber.Create(tenantPhoneNumber);
        var contactInfo = ContactInfo.Create(email, phoneNumber);

        var personalInfo = PersonalInfo.Create(tenantFirstName, tenantLastName);

        return Tenant.Create(
            personalInfo,
            contactInfo);
    }
}

