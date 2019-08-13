using System;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace LeanCode.Logging.Tests
{
    public class BaseSanitizerTests
    {
        private readonly ILogEventPropertyValueFactory factory;

        public BaseSanitizerTests()
        {
            factory = Substitute.For<ILogEventPropertyValueFactory>();
        }

        [Fact]
        public void Returns_false_and_sets_the_result_to_null_when_sanitizer_does_nothing()
        {
            var (res, result) = SanitizeRight(Payload.Sanitized);

            Assert.False(res);
            Assert.Null(result);
        }

        [Fact]
        public void Throws_when_Sanitizer_returns_the_same_object()
        {
            Assert.Throws<InvalidOperationException>(() => SanitizeWrong(Payload.Workable));
        }

        [Fact]
        public void Calls_property_factory_when_sanitizing_object()
        {
            SanitizeRight(Payload.Workable);

            factory.Received(1).CreatePropertyValue(RightSanitizer.Result, true);
        }

        [Fact]
        public void Returns_true_with_factory_result_when_sanitizing_object()
        {
            var expResult = Substitute.For<LogEventPropertyValue>();
            factory.CreatePropertyValue(default, default).ReturnsForAnyArgs(expResult);

            var (res, result) = SanitizeRight(Payload.Workable);

            Assert.True(res);
            Assert.Equal(expResult, result);
        }

        private (bool, LogEventPropertyValue) SanitizeRight(Payload p)
        {
            var res = new RightSanitizer().TryDestructure(p, factory, out var obj);
            return (res, obj);
        }

        private (bool, LogEventPropertyValue) SanitizeWrong(Payload p)
        {
            var res = new WrongSanitizer().TryDestructure(p, factory, out var obj);
            return (res, obj);
        }
    }

    public class RightSanitizer : BaseSanitizer<Payload>
    {
        public static readonly Payload Result = new Payload { Value = Placeholder };

        protected override Payload TrySanitize(Payload obj)
        {
            if (obj.Value != Placeholder)
            {
                return Result;
            }
            else
            {
                return null;
            }
        }
    }

    public class WrongSanitizer : BaseSanitizer<Payload>
    {
        protected override Payload TrySanitize(Payload obj)
        {
            if (obj.Value != Placeholder)
            {
                obj.Value = Placeholder;
                return obj;
            }
            else
            {
                return null;
            }
        }
    }

    public class Payload
    {
        public static readonly Payload Workable = new Payload { Value = "NOT PLACEHOLDER" };
        public static readonly Payload Sanitized = new Payload { Value = RightSanitizer.Placeholder };

        public string Value { get; set; }
    }
}
