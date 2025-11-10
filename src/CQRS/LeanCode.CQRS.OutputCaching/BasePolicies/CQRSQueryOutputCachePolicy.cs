using LeanCode.Contracts;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSQueryOutputCachePolicy<TQuery, TResult>
    : CQRSOutputCachePolicy<TQuery>,
        ICQRSQueryOutputCachePolicy<TQuery, TResult>
    where TQuery : IQuery<TResult>;
