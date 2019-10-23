using System;
using System.Net;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
            : base("Cannot execute the CQRS call because the request was unauthorized. Check your access token.") { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base("Cannot execute the CQRS call because the server forbid the request. Check if you have enough privileges.") { }
    }

    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException()
            : base("The server was unable to process the request and returned 500 Internal Server Error.") { }
    }

    public class QueryNotFoundException : Exception
    {
        public QueryNotFoundException()
            : base("The query does not exist. Check if you have up-to-date contracts.") { }
    }

    public class InvalidQueryException : Exception
    {
        public InvalidQueryException()
            : base("The server was unable to process the query because the content was malformed.") { }
    }

    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException()
            : base("The command does not exist. Check if you have up-to-date contracts.") { }
    }

    public class InvalidCommandException : Exception
    {
        public InvalidCommandException()
            : base("The server was unable to process the command because the content was malformed.") { }
    }

    public class MalformedResponseException : Exception
    {
        public MalformedResponseException()
            : base("The server returned malformed response.") { }

        public MalformedResponseException(Exception innerException)
            : base("The server returned malformed response.", innerException) { }
    }

    public class HttpCallErrorException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpCallErrorException(HttpStatusCode statusCode)
            : base($"Server rejected the request with {statusCode} HTTP code.")
        {
            StatusCode = statusCode;
        }
    }
}
