using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public static class Ensure
{
    public static void NotNullOrWhiteSpace(
      string? value,
      string? message = null,
      [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message ?? "The value can't be null or empty", paraName);
        }
    }

    public static void NotEmptyGuid(
       Guid value,
       string? message = null,
       [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message ?? "The value can't be null or empty", paraName);
        }
    }

    public static void CharactersExactLength(
       string value,
       int length,
       string? message = null,
       [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value.Length != length)
        {
            throw new ArgumentException(message ?? "The characters should be in exact length", paraName);
        }
    }

    public static void NotNull(
        object? value,
        string? message = null,
        [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value is null)
        {
            throw new ArgumentException(message ?? "The value can't be null", paraName);
        }
    }

    public static void InRangeInteger(
        int value,
        int from,
        int to,
        string? message = null,
        [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value < from || value > to)
        {
            throw new ArgumentOutOfRangeException(paraName, value, message ?? "The value can't be out of range");
        }
    }

    public static void NonNegativeDecimal(
        decimal value,
        string? message = null,
        [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paraName, value, message ?? "The value can't be out of range");
        }
    }

    public static void NonNegativeInteger(
        int value,
        string? message = null,
        [CallerArgumentExpression("value")] string? paraName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paraName, value, message ?? "The value can't be out of range");
        }
    }
}
