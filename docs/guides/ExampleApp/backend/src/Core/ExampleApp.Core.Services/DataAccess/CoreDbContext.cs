using ExampleApp.Core.Domain.Projects;
using ExampleApp.Core.Services.DataAccess.Entities;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.EF;
using LeanCode.DomainModels.MassTransitRelay.Inbox;
using LeanCode.DomainModels.MassTransitRelay.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Core.Services.DataAccess;

public class CoreDbContext : IdentityDbContext<AuthUser, AuthRole, Guid>, IOutboxContext, IConsumedMessagesContext
{
    public DbContext Self => this;
    public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();
    public DbSet<RaisedEvent> RaisedEvents => Set<RaisedEvent>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Employee> Employees => Set<Employee>();

    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("dbo");

        ConsumedMessage.Configure(builder);
        RaisedEvent.Configure(builder);

        ConfigureAuth(builder);

        builder.Entity<Employee>(e =>
        {
            e.HasKey(t => t.Id);

            e.Property(t => t.Id).IsTypedId();

            e.Property(t => t.Name).HasMaxLength(500);
            e.Property(t => t.Email).HasMaxLength(500);
        });

        builder.Entity<Project>(e =>
        {
            e.HasKey(t => t.Id);

            e.Property(t => t.Id).IsTypedId();

            e.Property(t => t.Name).HasMaxLength(500);

            e.OwnsMany(
                p => p.Assignments,
                inner =>
                {
                    inner.WithOwner(a => a.ParentProject).HasForeignKey(a => a.ParentProjectId);

                    inner.Property(a => a.Name).HasMaxLength(500);
                    inner.Property(a => a.Id).IsTypedId();
                    inner.Property(a => a.ParentProjectId).IsTypedId();
                    inner.Property(a => a.AssignedEmployeeId).IsTypedId();

                    inner.HasKey(a => new { a.Id }).IsClustered(false);
                    inner.HasIndex(a => new { a.ParentProjectId, a.Id }).IsClustered(true);

                    inner.ToTable("Assignments");
                }
            );
        });
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);

    private static void ConfigureAuth(ModelBuilder builder)
    {
        builder.Entity<AuthUser>(b =>
        {
            b.HasMany(u => u.Claims).WithOne().HasPrincipalKey(e => e.Id).HasForeignKey(e => e.UserId);
            b.ToTable("AspNetUsers", "auth");
        });
        builder.Entity<AuthRole>(b => b.ToTable("AspNetRoles", "auth"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("AspNetUserClaims", "auth"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("AspNetRoleClaims", "auth"));
        builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("AspNetUserRoles", "auth"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("AspNetUserLogins", "auth"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("AspNetUserTokens", "auth"));
    }
}
