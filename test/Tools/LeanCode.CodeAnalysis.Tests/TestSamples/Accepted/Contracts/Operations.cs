using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.Contracts;

public abstract class BaseOperation : IOperation<bool> { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class FirstOperation : BaseOperation { }

public class SecondOperation : FirstOperation { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class ThirdOperation : IOperation { }

[AllowUnauthorized]
public class UnauthorizedOperation : IOperation { }
