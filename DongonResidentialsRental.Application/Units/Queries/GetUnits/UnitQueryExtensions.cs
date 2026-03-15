using DongonResidentialsRental.Domain.Building;
using DongonResidentialsRental.Domain.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Units.Queries.GetUnits;

public static class UnitQueryExtensions
{
    public static IQueryable<Unit> ApplyStatusFilter(
        this IQueryable<Unit> query,
        UnitStatus? status)
    {
        if (status is null)
            return query;

        return query.Where(u => u.Status == status.Value);
    }

    public static IQueryable<Unit> ApplyUnitNumberSearch(
        this IQueryable<Unit> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        searchTerm = searchTerm.Trim();
        var pattern = $"%{searchTerm}%";

        return query.Where(u => EF.Functions.Like(u.UnitNumber, pattern));
    }

    public static IQueryable<Unit> ApplyFloorFilter(
        this IQueryable<Unit> query,
        int? floor)
    {

        if (floor is null && floor < 1)
            return query;

        return query.Where(u => u.Floor == floor);
    }

    public static IQueryable<Unit> ApplyBuildingFilter(
        this IQueryable<Unit> query,
        BuildingId? buildingId)
    {

        if (buildingId is null || buildingId.Id == Guid.Empty)
            return query;

        return query.Where(u => u.BuildingId == buildingId);
    }

    public static IQueryable<Unit> ApplyOrdering(this IQueryable<Unit> query)
    {
        return query
            .OrderBy(u => u.UnitNumber);
    }
}
