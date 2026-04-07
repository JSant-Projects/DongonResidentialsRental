using DongonResidentialsRental.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record PhoneNumber
{
    public string Value { get; }
    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        Ensure.NotNullOrWhiteSpace(phoneNumber, "PhoneNumber cannot be null or empty");
        if (!IsValidPhoneNumber(phoneNumber))
        {
            throw new DomainException("Invalid phone number");
        }
        return new PhoneNumber(phoneNumber);
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        var regex = new Regex(@"^(?:09\d{9}|\+639\d{9})$");
        return regex.IsMatch(phoneNumber);
    }
}
