using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.ExternalIdentityProviders.Tests;

public class User : IdentityUser<Guid> { }

public class Role : IdentityRole<Guid> { }

public static class UserManager
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA2000",
        Justification = "References don't go out of scope."
    )]
    public static UserManager<User> PrepareInMemory()
    {
        var dbContext = new IdentityDbContext<User, Role, Guid>(
            new DbContextOptionsBuilder<IdentityDbContext<User, Role, Guid>>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options
        );

        dbContext.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityCore<User>();
        services.AddSingleton<IUserStore<User>>(
            new UserStore<User, Role, IdentityDbContext<User, Role, Guid>, Guid>(dbContext)
        );
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<UserManager<User>>();
    }

    public static async Task<Guid> AddUserAsync(this UserManager<User> users)
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, UserName = id.ToString("N"), };
        await users.CreateAsync(user);
        return id;
    }
}
