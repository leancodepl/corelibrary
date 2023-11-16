using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.CodeAnalysis.Tests.TestSamples;

public abstract class BaseQuery : IQuery<bool> { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class FirstQuery : BaseQuery { }

public class SecondQuery : FirstQuery { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class ThirdQuery : IQuery { }

[AllowUnauthorized]
public class UnauthorizedQuery : IQuery { }
