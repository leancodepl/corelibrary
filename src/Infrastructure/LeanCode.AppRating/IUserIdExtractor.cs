using Microsoft.AspNetCore.Http;

namespace LeanCode.AppRating;

public interface IUserIdExtractor<TUserId>
{
    public TUserId Extract(HttpContext httpContext);
}
