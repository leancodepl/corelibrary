using Microsoft.AspNetCore.Builder;

namespace LeanCode.Components
{
    public interface IWebApplication : IAppComponent
    {
        string BasePath { get; }
        void Configure(IApplicationBuilder app);
    }
}
