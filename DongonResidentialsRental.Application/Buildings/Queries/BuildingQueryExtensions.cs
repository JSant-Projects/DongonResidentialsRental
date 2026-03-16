using DongonResidentialsRental.Domain.Building;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Buildings.Queries;

public static class BuildingQueryExtensions
{
    public static IQueryable<Building> ApplyStatusFilter(
        this IQueryable<Building> query,
        BuildingStatus? status)
    {
        if (status is null)
            return query;

        return query.Where(b => b.Status == status.Value);
    }

    public static IQueryable<Building> ApplySearch(
        this IQueryable<Building> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        searchTerm = searchTerm.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(b =>
            EF.Functions.Like(b.Name, pattern) ||
            EF.Functions.Like(b.Address.Street, pattern) ||
            EF.Functions.Like(b.Address.City, pattern) ||
            EF.Functions.Like(b.Address.Province, pattern));
    }

    public static IQueryable<Building> ApplyOrdering(this IQueryable<Building> query)
    {
        return query
            .OrderBy(b => b.Name);
    }
}
