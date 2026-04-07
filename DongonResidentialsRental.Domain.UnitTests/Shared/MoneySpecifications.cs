using AwesomeAssertions;
using DongonResidentialsRental.Domain.Shared;
using DongonResidentialsRental.Domain.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DongonResidentialsRental.Domain.UnitTests.Shared;

public class MoneySpecifications
{
    [Theory]
    [InlineData("USD", 100)]
    [InlineData("EUR", 50.5)]
    [InlineData("JPY", 1000)]
    public void Create_Should_Return_Money_When_Currency_And_Amount_Are_Valid(string currency, decimal amount)
    {
        var result = Money.Create(currency, amount);
        result.Should().BeOfType<Money>();
        result.Should().NotBeNull();
        result.Currency.Should().Be(currency);
        result.Amount.Should().Be(amount);
    }

    [Fact]
    public void Zero_Should_Return_Money_With_Zero_Amount_When_Currency_Is_Valid()
    {
        var result = Money.Zero("USD");
        result.Should().BeOfType<Money>();
        result.Should().NotBeNull();
        result.Currency.Should().Be("USD");
        result.Amount.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_Should_Throw_DomainException_When_Currency_Length_Is_Empty_Or_Null(string currency)
    {
        Action act = () => Money.Create(currency, 50.5m);
        act.Should().ThrowExactly<DomainException>().WithMessage($"Currency cannot be null or empty*");
    }

    [Theory]
    [InlineData("USDD")]
    [InlineData("EU")]
    [InlineData("JPYYYYYY")]
    public void Create_Should_Throw_DomainException_When_Currency_Length_Is_Invalid(string currency)
    {
        Action act = () => Money.Create(currency, 1000);
        act.Should().ThrowExactly<DomainException>().WithMessage($"Currency must be a 3-letter ISO code*");
    }

    [Theory]
    [InlineData(-1)]
    public void Create_Should_Throw_DomainException_When_Amount_Is_Negative(decimal amount)
    {
        Action act = () => Money.Create("USD", amount);
        act.Should().ThrowExactly<DomainException>().WithMessage($"Amount cannot be negative*");
    }


    [Theory]
    [InlineData("USD", 100, 100, 200)]
    [InlineData("EUR", 100, 50.5, 150.5)]
    [InlineData("JPY", 100, 1000, 1100)]
    public void Add_Should_Increment_Money_When_Other_Is_Valid(string currency, decimal originalAmount, decimal amount, decimal totalAmount)
    {
        var initialMoney = Money.Create(currency, originalAmount);
        var result = initialMoney.Add(Money.Create(currency, amount));
        result.Amount.Should().Be(totalAmount);
    }

    [Theory]
    [InlineData("USD", "JPY")]
    [InlineData("EUR", "USD")]
    [InlineData("JPY", "PHP")]
    public void Add_Should_Throw_DomainException_When_Other_Currency_Is_Different(string currency, string otherCurrency)
    {
        var initialMoney = Money.Create(currency, 100);
        Action act = () => initialMoney.Add(Money.Create(otherCurrency, 100));
        act.Should().ThrowExactly<DomainException>().WithMessage("Cannot operate on money with different currencies");
    }

    [Theory]
    [InlineData("USD", 1000, 100, 900)]
    [InlineData("EUR", 1000, 50.5, 949.5)]
    [InlineData("JPY", 1000, 500, 500)]
    public void Subtract_Should_Reduce_Money_When_Other_Is_Valid(string currency, decimal originalAmount, decimal amount, decimal totalAmount)
    {
        var initialMoney = Money.Create(currency, originalAmount);
        var result = initialMoney.Subtract(Money.Create(currency, amount));
        result.Amount.Should().Be(totalAmount);
    }

    [Theory]
    [InlineData("USD", "JPY")]
    [InlineData("EUR", "USD")]
    [InlineData("JPY", "PHP")]
    public void Subtract_Should_Throw_DomainException_When_Other_Currency_Is_Different(string currency, string otherCurrency)
    {
        var initialMoney = Money.Create(currency, 100);
        Action act = () => initialMoney.Subtract(Money.Create(otherCurrency, 100));
        act.Should().ThrowExactly<DomainException>().WithMessage("Cannot operate on money with different currencies");
    }

    [Theory]
    [InlineData("USD", 100, 2, 200)]
    [InlineData("CAD", 75.5, 3, 226.5)]
    public void Multiply_Should_Return_Money_With_Multiplied_Amount_When_Factor_Is_Valid(string currency, decimal originalAmount, decimal factor, decimal totalAmount)
    {
        var initialMoney = Money.Create(currency, originalAmount);
        var result = initialMoney.Multiply(factor);
        result.Amount.Should().Be(totalAmount);
    }
}