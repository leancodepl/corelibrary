#nullable enable
using FluentAssertions;
using LeanCode.QueryableExtensions;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class CollectionExtensionsTests
{
    private static readonly Animal Dog = new("dog", 4);
    private static readonly Species DogS1 = new("dog", 1);
    private static readonly Species DogS2 = new("dog", 2);
    private static readonly Animal Cat = new("cat", 4);
    private static readonly Species CatS = new("cat", 3);
    private static readonly Animal Dash = new("dash", 2);
    private static readonly Animal Parrot = new("parrot", 2);
    private static readonly Species ParrotS4 = new("parrot", 4);
    private static readonly Species ParrotS5 = new("parrot", 5);
    private static readonly Species ParrotS6 = new("parrot", 6);
    private readonly IEnumerable<Animal> animals = new[] { Parrot, Dog, Dash, Cat, Dog, Dash, };
    private readonly IEnumerable<Species> species = new[] { DogS1, DogS2, CatS, ParrotS4, ParrotS5, ParrotS6, };

    [Fact]
    public void ConditionalWhere_does_nothing_when_the_predicate_is_false()
    {
        animals.ConditionalWhere(a => a.Name == Parrot.Name, false).Should().BeEquivalentTo(animals);
        animals.AsQueryable().ConditionalWhere(a => a.Name == Parrot.Name, false).Should().BeEquivalentTo(animals);
    }

    [Fact]
    public void ConditionalWhere_filters_when_the_predicate_is_true()
    {
        animals.ConditionalWhere(a => a.Name == Parrot.Name, true).Should().ContainSingle().Which.Should().Be(Parrot);
        animals
            .AsQueryable()
            .ConditionalWhere(a => a.Name == Parrot.Name, true)
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(Parrot);
    }

    [Fact]
    public void OrderBy_orders_in_ascending_order()
    {
        animals.OrderBy(c => c.Name, false).Should().BeInAscendingOrder(a => a.Name);
        animals.AsQueryable().OrderBy(c => c.Name, false).Should().BeInAscendingOrder(a => a.Name);
    }

    [Fact]
    public void OrderBy_orders_in_descending_order()
    {
        animals.OrderBy(c => c.Name, true).Should().BeInDescendingOrder(a => a.Name);
        animals.AsQueryable().OrderBy(c => c.Name, true).Should().BeInDescendingOrder(a => a.Name);
    }

    [Fact]
    public void OrderBy_orders_correctly_using_custom_comparer()
    {
        animals
            .OrderBy(c => c.Name, false, new ReverseComparer())
            .Should()
            .BeEquivalentTo(new[] { Dog, Dog, Dash, Dash, Cat, Parrot, });
        animals
            .OrderBy(c => c.Name, true, new ReverseComparer())
            .Should()
            .BeEquivalentTo(new[] { Parrot, Cat, Dash, Dash, Dog, Dog, });
    }

    [Fact]
    public void ThenBy_orders_by_secondary_condition_correctly()
    {
        animals
            .OrderBy(c => c.LegsCount, false)
            .ThenBy(c => c.Name, true)
            .Should()
            .BeEquivalentTo(new[] { Parrot, Dash, Dash, Dog, Dog, Cat, });
        animals
            .OrderBy(c => c.LegsCount, true)
            .ThenBy(c => c.Name, false)
            .Should()
            .BeEquivalentTo(new[] { Cat, Dog, Dog, Dash, Dash, Parrot, });

        animals
            .AsQueryable()
            .OrderBy(c => c.LegsCount, false)
            .ThenBy(c => c.Name, true)
            .Should()
            .BeEquivalentTo(new[] { Parrot, Dash, Dash, Dog, Dog, Cat, });
        animals
            .AsQueryable()
            .OrderBy(c => c.LegsCount, true)
            .ThenBy(c => c.Name, false)
            .Should()
            .BeEquivalentTo(new[] { Cat, Dog, Dog, Dash, Dash, Parrot, });
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
                    new { Animal = Parrot, Species = (Species?)ParrotS4, },
                    new { Animal = Parrot, Species = (Species?)ParrotS5, },
                    new { Animal = Parrot, Species = (Species?)ParrotS6, },
                    new { Animal = Dog, Species = (Species?)DogS1, },
                    new { Animal = Dog, Species = (Species?)DogS2, },
                    new { Animal = Dash, Species = null as Species, },
                    new { Animal = Cat, Species = (Species?)CatS, },
                    new { Animal = Dog, Species = (Species?)DogS1, },
                    new { Animal = Dog, Species = (Species?)DogS2, },
                    new { Animal = Dash, Species = null as Species, },
                }
            );
        animals
            .AsQueryable()
            .LeftJoin(species.AsQueryable(), a => a.Name, s => s.AnimalName, (a, s) => new { Animal = a, Species = s })
            .Should()
            .BeEquivalentTo(
                new[]
                {
                    new { Animal = Parrot, Species = (Species?)ParrotS4, },
                    new { Animal = Parrot, Species = (Species?)ParrotS5, },
                    new { Animal = Parrot, Species = (Species?)ParrotS6, },
                    new { Animal = Dog, Species = (Species?)DogS1, },
                    new { Animal = Dog, Species = (Species?)DogS2, },
                    new { Animal = Dash, Species = null as Species, },
                    new { Animal = Cat, Species = (Species?)CatS, },
                    new { Animal = Dog, Species = (Species?)DogS1, },
                    new { Animal = Dog, Species = (Species?)DogS2, },
                    new { Animal = Dash, Species = null as Species, },
                }
            );
    }
}

internal sealed class Animal(string name, int legsCount)
{
    public string Name { get; init; } = name;
    public int LegsCount { get; init; } = legsCount;
}

internal sealed class Species(string animalName, int index)
{
    public string AnimalName { get; init; } = animalName;
    public int Index { get; init; } = index;
}

public class ReverseComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        return new System.Collections.CaseInsensitiveComparer().Compare(Reverse(x), Reverse(y));
    }

    private static string? Reverse(string? s)
    {
        if (s is null)
        {
            return null;
        }

        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
