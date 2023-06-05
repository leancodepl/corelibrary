using System;
using System.Net.Http;
using static System.Net.HttpStatusCode;

namespace LeanCode.CQRS.RemoteHttp.Client;

public static class HttpResponseMessageExtensions
{
    public static void HandleCommonCQRSErrors<TNotFound, TBadRequest>(this HttpResponseMessage response)
        where TNotFound : Exception, new()
        where TBadRequest : Exception, new()
    {
        switch (response.StatusCode)
        {
            case NotFound:
                throw new TNotFound();
            case BadRequest:
                throw new TBadRequest();
            case InternalServerError:
                throw new InternalServerErrorException();
            case Unauthorized:
                throw new UnauthorizedException();
            case Forbidden:
                throw new ForbiddenException();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpCallErrorException(response.StatusCode);
        }
    }
}
