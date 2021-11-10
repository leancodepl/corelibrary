using System;

namespace LeanCode.DomainModels.Model
{
    public interface IOptimisticConcurrency
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1819", Justification = "Required by EFCore.")]
        byte[] RowVersion { get; set; }
        DateTime DateModified { get; set; }
    }
}
