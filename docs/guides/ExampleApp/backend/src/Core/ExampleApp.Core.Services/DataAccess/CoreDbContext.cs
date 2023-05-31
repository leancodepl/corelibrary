using ExampleApp.Core.Services.DataAccess.Entities;
using LeanCode.DomainModels.DataAccess;
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

    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("dbo");

        ConsumedMessage.Configure(builder);
        RaisedEvent.Configure(builder);

        ConfigureAuth(builder);
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
