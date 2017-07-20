using System;

namespace LeanCode.DomainModels.Model
{
    public interface IOptimisticConcurrency
    {
        byte[] RowVersion { get; set; }
        DateTime DateModified { get; set; }
    }
}
