using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.MSSQL
{
    public class MSSQLPushNotificationTokenEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = "";
        public DateTime DateCreated { get; set; }

        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<MSSQLPushNotificationTokenEntity>(c =>
            {
                c.HasKey(e => e.Id).IsClustered(false);
                c.HasIndex(e => e.UserId).IsClustered(true);
                c.HasIndex(e => e.Token).IsUnique(true);

                c.Property(e => e.Id).ValueGeneratedNever();
                c.Property(e => e.Token).IsRequired(true).HasMaxLength(512);
            });
        }
    }
}
