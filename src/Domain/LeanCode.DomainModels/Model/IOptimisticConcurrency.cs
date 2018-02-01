using System;

namespace LeanCode.DomainModels.Model
{
    public interface IOptimisticConcurrency
    {
        byte[] RowVersion { get; }
        DateTime DateModified { get; set; }
    }
}
