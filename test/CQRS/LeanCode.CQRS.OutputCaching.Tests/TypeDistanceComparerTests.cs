using FluentAssertions;
using LeanCode.CQRS.OutputCaching.Registration;
using Xunit;

namespace LeanCode.CQRS.OutputCaching.Tests;

public class TypeDistanceComparerTests
{
    [Fact]
    public void Picks_second_when_second_is_more_specific_class()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(ClassHierarchy.Base),
            typeof(ClassHierarchy.Intermediate),
            typeof(ClassHierarchy.MostDerived)
        );

        result.Should().Be(TypeDistanceComparison.SecondCloser);
    }

    [Fact]
    public void Picks_first_when_first_is_more_specific_class()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(ClassHierarchy.Intermediate),
            typeof(ClassHierarchy.Base),
            typeof(ClassHierarchy.MostDerived)
        );

        result.Should().Be(TypeDistanceComparison.FirstCloser);
    }

    [Fact]
    public void Picks_second_interface_when_it_implements_first()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(InterfaceHierarchy.IBase),
            typeof(InterfaceHierarchy.IIntermediate),
            typeof(InterfaceHierarchy.MostDerived)
        );

        result.Should().Be(TypeDistanceComparison.SecondCloser);
    }

    [Fact]
    public void Returns_equal_when_interfaces_are_not_assignable_to_each_other()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(CombinedInterface.IFirst),
            typeof(CombinedInterface.ISecond),
            typeof(CombinedInterface.Combined)
        );

        result.Should().Be(TypeDistanceComparison.Equal);
    }

    [Fact]
    public void Prefers_type_with_shorter_distance_when_no_direct_assignability()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(MixedHierarchy.Base),
            typeof(MixedHierarchy.IMarker),
            typeof(MixedHierarchy.Derived)
        );

        result.Should().Be(TypeDistanceComparison.FirstCloser);
    }

    [Fact]
    public void Returns_equal_when_target_implements_first_interface_and_second_class()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(CombinedBase.IMarker),
            typeof(CombinedBase.Base),
            typeof(CombinedBase.Combined)
        );

        result.Should().Be(TypeDistanceComparison.Equal);
    }

    [Fact]
    public void Throws_when_types_are_not_assignable_from_target()
    {
        var act = () =>
            TypeDistanceComparer.Compare(typeof(string), typeof(IDisposable), typeof(MixedHierarchy.Derived));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Returns_equal_when_types_are_identical()
    {
        var result = TypeDistanceComparer.Compare(
            typeof(ClassHierarchy.Base),
            typeof(ClassHierarchy.Base),
            typeof(ClassHierarchy.MostDerived)
        );

        result.Should().Be(TypeDistanceComparison.Equal);
    }

    private static class ClassHierarchy
    {
        internal class Base;

        internal class Intermediate : Base;

        internal sealed class MostDerived : Intermediate;
    }

    private static class InterfaceHierarchy
    {
        internal interface IBase;

        internal interface IIntermediate : IBase;

        internal sealed class MostDerived : IIntermediate;
    }

    private static class CombinedInterface
    {
        internal interface IFirst;

        internal interface ISecond;

        internal class Combined : IFirst, ISecond;
    }

    private static class MixedHierarchy
    {
        internal interface IMarker;

        internal class Base : IMarker;

        internal sealed class Derived : Base;
    }

    private static class CombinedBase
    {
        internal interface IMarker;

        internal class Base;

        internal sealed class Combined : Base, IMarker;
    }
}
