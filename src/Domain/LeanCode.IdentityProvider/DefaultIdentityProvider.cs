using System;

namespace LeanCode.IdentityProvider
{
    public sealed class DefaultIdentityProvider : IIdentityProvider
    {
        public Guid NewId() => Guid.NewGuid();
    }
}
