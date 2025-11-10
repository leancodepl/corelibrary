using LeanCode.Contracts;

namespace LeanCode.CQRS.OutputCaching.BasePolicies;

public abstract class CQRSOperationOutputCachePolicy<TOperation, TResult>
    : CQRSOutputCachePolicy<TOperation>,
        ICQRSOperationOutputCachePolicy<TOperation, TResult>
    where TOperation : IOperation<TResult>;
