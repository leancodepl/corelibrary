using System;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    // Source: https://github.com/IdentityServer/IdentityServer4.EntityFramework/blob/dev/src/IdentityServer4.EntityFramework/Entities/PersistedGrant.cs
    public class PersistedGrantEntity
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? Expiration { get; set; }
        public string Data { get; set; }

        public static void Configure(ModelBuilder builder)
        {
            // Source: https://github.com/IdentityServer/IdentityServer4.EntityFramework/blob/dev/src/IdentityServer4.EntityFramework/Extensions/ModelBuilderExtensions.cs
            builder.Entity<PersistedGrantEntity>(grant =>
            {
                grant.Property(x => x.Key).HasMaxLength(200).ValueGeneratedNever();
                grant.Property(x => x.Type).HasMaxLength(50).IsRequired();
                grant.Property(x => x.SubjectId).HasMaxLength(200);
                grant.Property(x => x.ClientId).HasMaxLength(200).IsRequired();
                grant.Property(x => x.CreationTime).IsRequired();
                grant.Property(x => x.Data).HasMaxLength(50000).IsRequired();

                grant.HasKey(x => x.Key);

                grant.HasIndex(x => new { x.SubjectId, x.ClientId, x.Type });
            });
        }
    }
}
