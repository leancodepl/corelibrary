using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.Firebase.FCM;

public static class ModelBuilderExtensions
{
    public static void ConfigurePushNotificationTokenEntity<TUserId>(
        this ModelBuilder builder,
        bool setTokenColumnMaxLength
    )
        where TUserId : notnull, IEquatable<TUserId>
    {
        builder.Entity<PushNotificationTokenEntity<TUserId>>(c =>
        {
            c.HasKey(e => new { e.UserId, e.Token });
            c.HasIndex(e => e.Token).IsUnique(true);

            c.Property(e => e.UserId).ValueGeneratedNever();

            var t = c.Property(e => e.Token).ValueGeneratedNever();

            if (setTokenColumnMaxLength)
            {
                // https://stackoverflow.com/q/39959417
                t.HasMaxLength(512);
            }
        });
    }
}
