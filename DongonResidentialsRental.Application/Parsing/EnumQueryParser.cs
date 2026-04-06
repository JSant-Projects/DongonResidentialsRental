using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Parsing;

public static class EnumQueryParser
{
    public static TEnum? ParseNullable<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"Invalid {typeof(TEnum).Name} value '{value}'.",
            nameof(value));
    }

    public static TEnum ParseRequired<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{typeof(TEnum).Name} is required.",
                nameof(value));
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"Invalid {typeof(TEnum).Name} value '{value}'.",
            nameof(value));
    }
}
