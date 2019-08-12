using System;

namespace LeanCode.DomainModels.Model
{
    ///<summary>Represents an optimistically concurrent entity</summary>
    ///<remarks>Usually this interface should be implemented explicilty, as it exposes public setters</remarks>
    public interface IOptimisticConcurrency
    {
        /// <summary>Concurrency token managed by underlying database provider</summary>
        byte[] RowVersion { get; set; }

        /// <summary>Set by data access layer when performing any update on entity</summary>
        DateTime DateModified { get; set; }
    }
}
