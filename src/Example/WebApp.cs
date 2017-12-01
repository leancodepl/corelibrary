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
        {
            services.AddMvc();
        }

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

            var catalog = new TypesCatalog(typeof(Startup));
            app.Map("/api", cfg => cfg.UseRemoteCQRS<AppContext>(catalog, ctx => new AppContext { User = ctx.User }));

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
