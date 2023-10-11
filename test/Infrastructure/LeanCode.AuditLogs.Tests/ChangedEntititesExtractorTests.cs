using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;

namespace LeanCode.AuditLogs.Tests;

public class ChangedEntitiesExtractorTests
{
    private static readonly JsonSerializerOptions Options =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false,
        };

    private const string SomeId = "some_id";
    private readonly TestDbContext dbContext;

    public ChangedEntitiesExtractorTests()
    {
        dbContext = new TestDbContext();
    }

    [Fact]
    public void Check_if_added_entity_is_extracted()
    {
        var testEntity = TestEntity.Create(SomeId);
        dbContext.TestEntities.Add(testEntity);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    EntityState = "Added",
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }

    [Fact]
    public void Check_if_updated_entity_is_extracted()
    {
        dbContext.TestEntities.Add(TestEntity.Create(SomeId));

        dbContext.SaveChanges();

        var testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.SetSomeString("foo bar");
        dbContext.TestEntities.Update(testEntity);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Modified",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }

    [Fact]
    public void Check_if_deleted_entity_is_extracted()
    {
        dbContext.TestEntities.Add(TestEntity.Create(SomeId));
        dbContext.SaveChanges();

        var testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        dbContext.Remove(testEntity!);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Deleted",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }

    [Fact]
    public void Check_if_finds_multiple_changed_entities()
    {
        const string OtherId = "OtherId";
        dbContext.TestEntities.Add(TestEntity.Create(SomeId));
        dbContext.SaveChanges();

        var otherEntity = TestEntity.Create(OtherId);
        dbContext.TestEntities.Add(otherEntity);
        var testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.SetSomeString("foo bar");
        dbContext.TestEntities.Update(testEntity);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .HaveCount(2)
            .And.Subject.Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    EntityState = "Modified",
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                },
                opt => opt.ComparingByMembers<JsonElement>()
            )
            .And.Subject.Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { OtherId },
                    Type = typeof(TestEntity).FullName,
                    EntityState = "Added",
                    Changes = JsonSerializer.SerializeToDocument(otherEntity, Options),
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }

    [Fact]
    public void Check_if_root_entity_is_extracted_when_owned_entities_change()
    {
        dbContext.TestEntities.Add(TestEntity.Create(SomeId));
        dbContext.SaveChanges();

        var testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.AddOwnedEntities();
        dbContext.TestEntities.Update(testEntity);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Modified",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );

        dbContext.SaveChanges();

        testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.UpdateOwnedEntities();
        dbContext.TestEntities.Update(testEntity);

        changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Modified",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }

    [Fact]
    public void Check_if_root_entity_is_extracted_when_included_entities_change()
    {
        dbContext.TestEntities.Add(TestEntity.Create(SomeId));
        dbContext.SaveChanges();

        var testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.AddIncludedEntities();
        dbContext.TestEntities.Update(testEntity);

        var changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Modified",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );

        dbContext.SaveChanges();
        testEntity = dbContext.TestEntities.Find(SomeId);
        testEntity.Should().NotBeNull();
        testEntity!.UpdateIncludedEntities();
        dbContext.TestEntities.Update(testEntity);

        changes = ChangedEntitiesExtractor.Extract(dbContext);

        changes
            .Should()
            .ContainEquivalentOf(
                new
                {
                    Ids = new string[] { SomeId },
                    Type = typeof(TestEntity).FullName,
                    Changes = JsonSerializer.SerializeToDocument(testEntity, Options),
                    EntityState = "Modified",
                },
                opt => opt.ComparingByMembers<JsonElement>()
            );
    }
}
