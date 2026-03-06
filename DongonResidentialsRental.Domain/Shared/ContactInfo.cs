using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public record ContactInfo
{
    public Email Email { get; }
    public PhoneNumber PhoneNumber { get; }
    private ContactInfo(Email email, PhoneNumber phoneNumber)
    {
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static ContactInfo Create(Email email, PhoneNumber phoneNumber)
    {
        Ensure.NotNull(email, "Email cannot be null");
        Ensure.NotNull(phoneNumber, "Phone number cannot be null");
        return new ContactInfo(email, phoneNumber);
    }
}
