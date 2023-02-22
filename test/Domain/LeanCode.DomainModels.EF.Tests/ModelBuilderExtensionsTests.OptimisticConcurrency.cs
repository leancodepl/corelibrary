using System;
using System.Diagnostics.CodeAnalysis;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class ModelBuilderExtensionsTests_OptimisticConcurrency
{
    [Fact]
    public void When_IOC_is_implemented_explicitly_DateModified_has_correctly_configured_backing_field()
    {
        using (var db = new ExplicitlyContext())
        {
            var ent = db.Model.FindEntityType(typeof(ExplicitlyImplemented).FullName);
            var prop = ent.FindProperty("DateModified");
            Assert.Equal(
                "<LeanCode.DomainModels.Model.IOptimisticConcurrency.DateModified>k__BackingField",
                prop.FieldInfo.Name
            );
        }
    }

    [Fact]
    public void When_IOC_is_implemented_implicitly_DateModified_has_correctly_configured_backing_field()
    {
        using (var db = new ImplicitlyContext())
        {
            var ent = db.Model.FindEntityType(typeof(ImplicitlyImplemented).FullName);
            var prop = ent.FindProperty("DateModified");
            Assert.Equal("<DateModified>k__BackingField", prop.FieldInfo.Name);
        }
    }

    [Fact]
    public void When_DateModified_is_implemented_differently_Fails()
    {
        using (var db = new WrongDateModifiedContext())
        {
            Assert.Throws<InvalidOperationException>(() => db.Model.FindEntityType(typeof(WrongDateModified).FullName));
        }
    }
}

public class ExplicitlyImplemented : IOptimisticConcurrency
{
    public int Id { get; set; }
    DateTime IOptimisticConcurrency.DateModified { get; set; }
}

public class ExplicitlyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableOptimisticConcurrency<ExplicitlyImplemented>();
    }
}

public class ImplicitlyImplemented : IOptimisticConcurrency
{
    public int Id { get; set; }
    public DateTime DateModified { get; set; }
}

public class ImplicitlyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableOptimisticConcurrency<ImplicitlyImplemented>();
    }
}

[SuppressMessage("?", "IDE0032", Justification = "Specifically for tests.")]
public class WrongDateModified : IOptimisticConcurrency
{
    private DateTime dateModified;

    public int Id { get; set; }
    public DateTime DateModified
    {
        get => dateModified;
        set => dateModified = value;
    }
}

public class WrongDateModifiedContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableOptimisticConcurrency<WrongDateModified>();
    }
}
