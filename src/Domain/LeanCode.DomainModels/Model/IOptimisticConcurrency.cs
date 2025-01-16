namespace LeanCode.DomainModels.Model;

public interface IOptimisticConcurrency
{
    DateTime DateModified { get; set; }
}
