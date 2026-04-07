using DongonResidentialsRental.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record PostalCode
{
    public string Value { get; }
    private PostalCode(string value)
    {
        Value = value;
    }

    public static PostalCode Create(string postalCode)
    {
        Ensure.NotNullOrWhiteSpace(postalCode, "Postal code cannot be null or empty");
        if (!IsValidPostalCode(postalCode))
        {
            throw new DomainException("Invalid postal code");
        }
        return new PostalCode(postalCode);
    }

    private static bool IsValidPostalCode(string postalCode)
    {
        var regex = new Regex(@"^\d{4}$");
        return regex.IsMatch(postalCode);
    }
}
