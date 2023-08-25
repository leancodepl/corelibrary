using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using LeanCode.QueryableExtensions;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class CollectionExtensionsTests
{
    private static readonly Animal dog = new("dog", 4);
    private static readonly Species dogS1 = new("dog", 1);
    private static readonly Species dogS2 = new("dog", 2);
    private static readonly Animal cat = new("cat", 4);
    private static readonly Species catS = new("cat", 3);
    private static readonly Animal dash = new("dash", 2);
    private static readonly Animal parrot = new("parrot", 2);
    private static readonly Species parrotS4 = new("parrot", 4);
    private static readonly Species parrotS5 = new("parrot", 5);
    private static readonly Species parrotS6 = new("parrot", 6);
    private readonly IEnumerable<Animal> animals = new[] { parrot, dog, dash, cat, dog, dash, };
    private readonly IEnumerable<Species> species = new[] { dogS1, dogS2, catS, parrotS4, parrotS5, parrotS6, };

    [Fact]
    public void ConditionalWhere_does_nothing_when_the_predicate_is_false()
    {
        animals.ConditionalWhere(a => a.Name == parrot.Name, false).Should().BeEquivalentTo(animals);
    }

    [Fact]
    public void ConditionalWhere_filters_when_the_predicate_is_true()
    {
        animals.ConditionalWhere(a => a.Name == parrot.Name, true).Should().ContainSingle().Which.Should().Be(parrot);
    }

    [Fact]
    public void OrderBy_orders_in_ascending_order()
    {
        animals.OrderBy(c => c.Name, false).Should().BeInAscendingOrder(a => a.Name);
    }

    [Fact]
    public void OrderBy_orders_in_descending_order()
    {
        animals.OrderBy(c => c.Name, true).Should().BeInDescendingOrder(a => a.Name);
    }

    [Fact]
    public void ThenBy_orders_by_secondary_condition_correctly()
    {
        animals
            .OrderBy(c => c.LegsCount, false)
            .ThenBy(c => c.Name, true)
            .Should()
            .BeEquivalentTo(new[] { parrot, dash, dash, dog, dog, cat, });
        animals
            .OrderBy(c => c.LegsCount, true)
            .ThenBy(c => c.Name, false)
            .Should()
            .BeEquivalentTo(new[] { cat, dog, dog, dash, dash, parrot, });
    }

    [Fact]
    public void LeftJoin_works_correctly()
    {
        animals
            .LeftJoin(species, a => a.Name, s => s.AnimalName, (a, s) => new { Animal = a, Species = s })
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    new { Animal = parrot, Species = parrotS4, },
                    new { Animal = parrot, Species = parrotS5, },
                    new { Animal = parrot, Species = parrotS6, },
                    new { Animal = dog, Species = dogS1, },
                    new { Animal = dog, Species = dogS2, },
                    new { Animal = dash, Species = null as Species, },
                    new { Animal = cat, Species = catS, },
                    new { Animal = dog, Species = dogS1, },
                    new { Animal = dog, Species = dogS2, },
                    new { Animal = dash, Species = null as Species, },
                }
            );
    }
}

internal class Animal(string name, int legsCount)
{
    public string Name { get; init; } = name;
    public int LegsCount { get; init; } = legsCount;
}

internal class Species(string animalName, int index)
{
    public string AnimalName { get; init; } = animalName;
    public int Index { get; init; } = index;
}
