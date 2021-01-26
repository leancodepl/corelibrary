using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ExternalIdentityProviders.Tests
{
    public class User : IdentityUser<Guid>
    { }

    public class Role : IdentityRole<Guid>
    { }

    public static class UserManager
    {
        public static UserManager<User> PrepareInMemory()
        {
            var dbContext = new IdentityDbContext<User, Role, Guid>(
                new DbContextOptionsBuilder<IdentityDbContext<User, Role, Guid>>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);

            dbContext.Database.EnsureCreated();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddIdentityCore<User>();
            services.AddSingleton<IUserStore<User>>(new UserStore<User, Role, IdentityDbContext<User, Role, Guid>, Guid>(dbContext));
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<UserManager<User>>();
        }
    }
}
