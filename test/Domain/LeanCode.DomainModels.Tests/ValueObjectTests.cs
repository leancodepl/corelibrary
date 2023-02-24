using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests;

public class ValueObjectTests
{
    [Fact]
    public void Equality_Operator_Works_With_Non_Nullable_Types()
    {
        Assert.True(TenPLN() == TenPLN());
        Assert.False(TenPLN() == TwentyPLN());
        Assert.False(TenPLN() == TenUSD());
    }

    [Fact]
    public void Equality_Operator_Works_With_Nullable_Types()
    {
        Assert.True(MaybeTenPLN(false) == MaybeTenPLN(false));
        Assert.True(MaybeTenPLN(true) == MaybeTenPLN(true));
        Assert.True(MaybeTenPLN(true) == MaybeTenUSD(true));
        Assert.False(MaybeTenPLN(false) == MaybeTwentyPLN(false));
        Assert.False(MaybeTenPLN(false) == MaybeTenUSD(false));
    }

    [Fact]
    public void Inequality_Operator_Works_With_Non_Nullable_Types()
    {
        Assert.False(TenPLN() != TenPLN());
        Assert.True(TenPLN() != TwentyPLN());
        Assert.True(TenPLN() != TenUSD());
    }

    [Fact]
    public void Inequality_Operator_Works_With_Nullable_Types()
    {
        Assert.False(MaybeTenPLN(false) != MaybeTenPLN(false));
        Assert.False(MaybeTenPLN(true) != MaybeTenPLN(true));
        Assert.False(MaybeTenPLN(true) != MaybeTenUSD(true));
        Assert.True(MaybeTenPLN(false) != MaybeTwentyPLN(false));
        Assert.True(MaybeTenPLN(false) != MaybeTenUSD(false));
    }

    [Fact]
    public void Equals_Method_Works()
    {
        Assert.True(TenPLN().Equals(TenPLN()));
        Assert.False(TenPLN().Equals(TwentyPLN()));
        Assert.False(TenPLN().Equals(TenUSD()));
    }

    [Fact]
    public void Hash_Codes_Are_Calculated_Properly()
    {
        Assert.True(TenPLN().GetHashCode() == TenPLN().GetHashCode());
        Assert.False(TenPLN().GetHashCode() == TwentyPLN().GetHashCode());
        Assert.False(TenPLN().GetHashCode() == TenUSD().GetHashCode());
    }

    private static Money TenPLN() => new Money(10, "PLN");

    private static Money TwentyPLN() => new Money(20, "PLN");

    private static Money TenUSD() => new Money(10, "USD");

    private static Money? MaybeTenPLN(bool @null) => @null ? null : new Money(10, "PLN");

    private static Money? MaybeTwentyPLN(bool @null) => @null ? null : new Money(20, "PLN");

    private static Money? MaybeTenUSD(bool @null) => @null ? null : new Money(10, "USD");

    private record Money : ValueObject
    {
        public decimal Amount { get; private init; }
        public string Currency { get; private init; } = "";

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
    }
}
