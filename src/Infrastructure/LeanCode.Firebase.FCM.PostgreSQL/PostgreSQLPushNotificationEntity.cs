using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.PostgreSQL
{
    public class PostgreSQLPushNotificationEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = "";
        public DateTime DateCreated { get; set; }

        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<PostgreSQLPushNotificationEntity>(c =>
            {
                c.HasKey(e => e.Id);
                c.HasIndex(e => e.UserId);
                c.HasIndex(e => e.Token).IsUnique(true);

                c.Property(e => e.Id).ValueGeneratedNever();
                c.Property(e => e.Token).IsRequired(true).HasMaxLength(512);
            });
        }
    }
}
