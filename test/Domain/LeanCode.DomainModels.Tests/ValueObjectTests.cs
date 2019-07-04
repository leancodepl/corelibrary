using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.Tests
{
    public class ValueObjectTests
    {
        [Fact]
        public void Equality_Operator_Works()
        {
            Assert.True(TenPLN() == TenPLN());
            Assert.False(TenPLN() == TwentyPLN());
            Assert.False(TenPLN() == TenUSD());
        }

        [Fact]
        public void Inequality_Operator_Works()
        {
            Assert.False(TenPLN() != TenPLN());
            Assert.True(TenPLN() != TwentyPLN());
            Assert.True(TenPLN() != TenUSD());
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

        private Money TenPLN() =>
            new Money()
            {
                Amount = 10,
                Currency = "PLN"
            };

        private Money TwentyPLN() =>
            new Money()
            {
                Amount = 20,
                Currency = "PLN"
            };

        private Money TenUSD() =>
            new Money()
            {
                Amount = 10,
                Currency = "USD"
            };

        class Money : ValueObject<Money>
        {
            protected override object[] GetAttributesToIncludeInEqualityCheck() => new object[] { Amount, Currency };

            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
    }
}
