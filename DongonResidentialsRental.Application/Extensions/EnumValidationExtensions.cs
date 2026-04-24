using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Application.Extensions;

public static class EnumValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeEnumValue<T, TEnum>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool allowEmpty = true)
        where TEnum : struct, Enum
    {
        return ruleBuilder.Must(value =>
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return allowEmpty;
            }

            return Enum.TryParse<TEnum>(value, ignoreCase: true, out _);
        })
        .WithMessage(_ =>
        { 
            var validValues = string.Join(", ",
                Enum.GetNames(typeof(TEnum))
                    .Select(ToCamelCase));

            return $"Invalid {typeof(TEnum).Name}. Valid values are: {validValues}.";
        });
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
