using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.Shared;

public sealed record Money
{
    private Money(string currency, decimal amount)
    {

        Currency = currency.ToUpperInvariant();
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static Money Create(string currency, decimal amount)
    {
        Ensure.NotNullOrWhiteSpace(currency, "Currency cannot be null or empty");
        Ensure.CharactersExactLength(currency, 3, "Currency must be a 3-letter ISO code");
        Ensure.NonNegativeDecimal(amount, "Amount cannot be negative");

        return new Money(currency, amount);
    }

    public static Money Zero(string currency)
    {
        Ensure.NotNullOrWhiteSpace(currency, "Currency cannot be null or empty");
        Ensure.CharactersExactLength(currency, 3, "Currency must be a 3-letter ISO code");
        return new Money(currency, 0);
    }
    public string Currency { get; }
    public decimal Amount { get; }

    public Money Add(Money other)
    {
        Ensure.NotNull(other, "Other can't be null");

        EnsureSameCurrency(other);

        return new Money(Currency, Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        Ensure.NotNull(other, "Other can't be null");

        EnsureSameCurrency(other);

        return new Money(Currency, Amount - other.Amount);
    }

    public Money Multiply(decimal factor)
    {
        Ensure.NonNegativeDecimal(factor, "Factor cannot be negative");
        return new Money(Currency, Amount * factor);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot operate on money with different currencies");
    }

    public Money Negate()
    {
       return new Money(Currency, -Amount);
    }

    //public bool Equals(Money? other)
    //    => other is not null &&
    //       Currency == other.Currency &&
    //       Amount == other.Amount;
    //public override int GetHashCode()
    //    => HashCode.Combine(Currency, Amount);
}
