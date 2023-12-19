using LeanCode.DomainModels.Model;
using Xunit;

namespace LeanCode.DomainModels.Tests.Model;

[Obsolete("Tests for obsolete classes")]
public class TypedIdTests
{
    private sealed record Entity(Id<Entity> Id) : IEntity<Id<Entity>>;

    private sealed record IntEntity(IId<IntEntity> Id) : IEntity<IId<IntEntity>>;

    private sealed record LongEntity(LId<LongEntity> Id) : IEntity<LId<LongEntity>>;

    [Fact]
    public void Guid_id_comparison_operators_work()
    {
        var same1 = new Id<Entity>(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var same2 = new Id<Entity>(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var other = new Id<Entity>(Guid.Parse("00000000-0000-0000-0000-000000000002"));

        Assert.True(same1 == same2);
        Assert.False(same1 == other);
        Assert.True(same1 != other);
        Assert.False(same1 == other);
    }

    [Fact]
    public void Int_id_comparison_operators_work()
    {
        var same1 = new IId<IntEntity>(1);
        var same2 = new IId<IntEntity>(1);
        var other = new IId<IntEntity>(2);

        Assert.True(same1 == same2);
        Assert.False(same1 == other);
        Assert.True(same1 != other);
        Assert.False(same1 == other);
    }

    [Fact]
    public void Long_id_comparison_operators_work()
    {
        var same1 = new LId<LongEntity>(1);
        var same2 = new LId<LongEntity>(1);
        var other = new LId<LongEntity>(2);

        Assert.True(same1 == same2);
        Assert.False(same1 == other);
        Assert.True(same1 != other);
        Assert.False(same1 == other);
    }
}
