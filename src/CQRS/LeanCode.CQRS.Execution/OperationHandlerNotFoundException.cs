using System;

namespace LeanCode.CQRS.Execution;

public class OperationHandlerNotFoundException : Exception
{
    public OperationHandlerNotFoundException(Type operationType)
        : base($"Cannot find handler for operation {operationType.Name}.")
    {
        OperationType = operationType;
    }

    public Type OperationType { get; }
}
