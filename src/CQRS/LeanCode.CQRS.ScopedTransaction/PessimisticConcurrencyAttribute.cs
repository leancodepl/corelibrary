using System.Data;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.CQRS.ScopedTransaction;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PessimisticConcurrencyAttribute : Attribute
{
    /// <summary>
    /// Whether <see cref="ScopedTransactionMiddleware{TDbContext}"/> should explicitly call <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.
    /// </summary>
    public bool SaveChanges { get; }
    public IsolationLevel IsolationLevel { get; }

    public PessimisticConcurrencyAttribute(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        bool saveChanges = true
    )
    {
        SaveChanges = saveChanges;
        IsolationLevel = isolationLevel;
    }
}
