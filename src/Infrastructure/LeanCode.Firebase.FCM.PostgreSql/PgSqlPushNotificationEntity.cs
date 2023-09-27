using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.PostgreSql;

public class PgSqlPushNotificationEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Token { get; set; } = "";
    public DateTime DateCreated { get; set; }

    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<PgSqlPushNotificationEntity>(c =>
        {
            c.HasKey(e => e.Id);
            c.HasIndex(e => e.UserId);
            c.HasIndex(e => e.Token).IsUnique(true);

            c.Property(e => e.Id).ValueGeneratedNever();
            c.Property(e => e.Token).IsRequired(true);
        });
    }
}
