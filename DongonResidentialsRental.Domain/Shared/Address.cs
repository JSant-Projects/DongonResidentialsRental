using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string Province { get; }
    public string PostalCode { get; }

    private Address(string street, string city, string province, string postalCode)
    {
        Street = street;
        City = city;
        Province = province;
        PostalCode = postalCode;
    }

    public static Address Create(string street, string city, string province, string postalCode)
    {
        Ensure.NotNullOrWhiteSpace(street, "Street cannot be null or empty");
        Ensure.NotNullOrWhiteSpace(city, "City cannot be null or empty");
        Ensure.NotNullOrWhiteSpace(province, "Province cannot be null or empty");
        Ensure.NotNullOrWhiteSpace(postalCode, "Postal code cannot be null or empty");
        return new Address(street, city, province, postalCode);
    }

}
