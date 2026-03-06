using DongonResidentialsRental.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Tenant;

public sealed record PersonalInfo
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";
    private PersonalInfo(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static PersonalInfo Create(string firstName, string lastName)
    {
        Ensure.NotNullOrWhiteSpace(firstName, "First name cannot be null or empty");
        Ensure.NotNullOrWhiteSpace(lastName, "Last name cannot be null or empty");
        return new PersonalInfo(firstName, lastName);
    }

}
