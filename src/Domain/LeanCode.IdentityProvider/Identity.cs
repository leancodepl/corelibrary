using System;

namespace LeanCode.IdentityProvider
{
    public static class Identity
    {
        private static IIdentityProvider provider = new DefaultIdentityProvider();

        public static Guid NewId() => provider.NewId();

        public static void UseIdentityProvider(IIdentityProvider newProvider)
        {
            provider = newProvider ?? throw new ArgumentNullException(nameof(newProvider));
        }
    }
}
