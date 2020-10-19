using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests
{
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
        public void Inquality_Operator_Works_With_Nullable_Types()
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

        private static Money TenPLN() =>
            new Money()
            {
                Amount = 10,
                Currency = "PLN",
            };

        private static Money TwentyPLN() =>
            new Money()
            {
                Amount = 20,
                Currency = "PLN",
            };

        private static Money TenUSD() =>
            new Money()
            {
                Amount = 10,
                Currency = "USD",
            };

        private static Money? MaybeTenPLN(bool @null) =>
            @null ? null : new Money()
            {
                Amount = 10,
                Currency = "PLN",
            };

        private static Money? MaybeTwentyPLN(bool @null) =>
            @null ? null : new Money()
            {
                Amount = 20,
                Currency = "PLN",
            };

        private static Money? MaybeTenUSD(bool @null) =>
            @null ? null : new Money()
            {
                Amount = 10,
                Currency = "USD",
            };

        private class Money : ValueObject<Money>
        {
            protected override object[] GetAttributesToIncludeInEqualityCheck() => new object[] { Amount, Currency };

            public decimal Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
        }
    }
}
