using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Lease;
using DongonResidentialsRental.Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Tenants.Queries;

public static class TenantQueryExtensions
{
    public static IQueryable<Tenant> ApplySearch(
        this IQueryable<Tenant> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        searchTerm = searchTerm.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(t =>
            EF.Functions.Like(t.PersonalInfo.FirstName, pattern) ||
            EF.Functions.Like(t.PersonalInfo.LastName, pattern) ||
            EF.Functions.Like(t.ContactInfo.Email.Value, pattern) ||
            EF.Functions.Like(t.ContactInfo.PhoneNumber.Value, pattern));
    }

    public static IQueryable<Tenant> ApplyOrdering(this IQueryable<Tenant> query)
    {
        return query
            .OrderBy(t => t.PersonalInfo.LastName)
            .ThenBy(t => t.PersonalInfo.FirstName);
    }

    public static IQueryable<Tenant> WithoutActiveLease(
        this IQueryable<Tenant> query,
        IQueryable<Lease> leases,
        DateOnly today)
    {
        return query.Where(t =>
                !leases.Any(
                    l => l.Occupancy == t.TenantId &&
                    l.Term.StartDate <= today &&
                    l.Term.EndDate >= today));
    }
}
