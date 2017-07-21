using LeanCode.Components;
using LeanCode.CQRS.RemoteHttp.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.Example
{
    public class WebApp : IWebApplication
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostEnv;

        public string BasePath => "/";
        public Autofac.Core.IModule AutofacModule => null;
        public AutoMapper.Profile MapperProfile => null;

        public WebApp(IConfiguration configuration, IHostingEnvironment env)
        {
            this.configuration = configuration;
            this.hostEnv = env;
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public void Configure(IApplicationBuilder app)
        {
            if (hostEnv.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Map("/api", cfg => cfg.UseRemoteCQRS(typeof(Startup)));

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
