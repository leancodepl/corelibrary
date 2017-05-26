using System;
using System.Net;
using System.Net.Http;

namespace LeanCode.CQRS.RemoteHttp.Client
{
    static class Helpers
    {
        public static void HandleCommonCQRSErrors<TNotFound, TBadRequest>(this HttpResponseMessage response)
            where TNotFound : Exception, new()
            where TBadRequest : Exception, new()
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new TNotFound();
            }
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new TBadRequest();
            }
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new InternalServerErrorException();
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenException();
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpCallErrorException(response.StatusCode);
            }
        }
    }
}
