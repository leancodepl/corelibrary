using System.Diagnostics.CodeAnalysis;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.Contracts;

public abstract class BaseQuery : IQuery<bool> { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class FirstQuery : BaseQuery { }

public class SecondQuery : FirstQuery { }

[AuthorizeWhenHasAnyOf("Sample_permission")]
public class ThirdQuery : IQuery { }

[SuppressMessage("??", "CA1040", Justification = "Empty marker interface")]
public interface ICustomQueryAuthorizer { }

public sealed class CustomQueryAuthorizerAttribute : AuthorizeWhenAttribute<ICustomQueryAuthorizer> { }

[CustomQueryAuthorizer]
public class CustomAuthorizedQuery : IQuery { }

[AllowUnauthorized]
public class UnauthorizedQuery : IQuery { }
