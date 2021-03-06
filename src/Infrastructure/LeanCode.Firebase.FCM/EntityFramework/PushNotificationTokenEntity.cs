using System;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Firebase.FCM.EntityFramework
{
    public class PushNotificationTokenEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        public static void Configure(ModelBuilder builder)
        {
            builder.Entity<PushNotificationTokenEntity>(c =>
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
