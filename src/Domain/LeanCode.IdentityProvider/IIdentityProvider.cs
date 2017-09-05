using System;

namespace LeanCode.IdentityProvider
{
    public interface IIdentityProvider
    {
        Guid NewId();
    }
}
