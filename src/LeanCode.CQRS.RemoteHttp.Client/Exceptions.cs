using System;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class InternalServerErrorException : Exception { }

    public class QueryNotFoundException : Exception { }
    public class InvalidQueryException : Exception { }

    public class CommandNotFoundException : Exception { }
    public class InvalidCommandException : Exception { }
}
