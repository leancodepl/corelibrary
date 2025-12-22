using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

/// <inheritdoc />
public interface ICQRSOutputCachePolicy<TObject> : IOutputCachePolicy
    where TObject : notnull;
