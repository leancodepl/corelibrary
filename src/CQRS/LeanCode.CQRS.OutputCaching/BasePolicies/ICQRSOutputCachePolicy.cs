using Microsoft.AspNetCore.OutputCaching;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public interface ICQRSOutputCachePolicy<TObject> : IOutputCachePolicy
    where TObject : notnull;
