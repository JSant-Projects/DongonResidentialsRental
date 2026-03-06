using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record Email
{
    private Email(string value)
    {
        Value = value;
    }
    public string Value { get; }

    public static Email Create(string email)
    {
        Ensure.NotNullOrWhiteSpace(email, "Email cannot be null or empty");

        if (!IsValidEmail(email))
        {
            throw new DomainException("Invalid email");
        }

        return new Email(email);
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var mailAdderss = new MailAddress(value);
        }
        catch
        {
            return false;
        }

        return true;
    }
}
