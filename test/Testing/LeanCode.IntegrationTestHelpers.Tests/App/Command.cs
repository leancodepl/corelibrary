using System.Threading.Tasks;
using LeanCode.Contracts;
using LeanCode.CQRS.Execution;
using Microsoft.AspNetCore.Http;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class Command : ICommand
{
    public int Id { get; set; }
    public string? Data { get; set; }
}

public class CommandCH : ICommandHandler<Command>
{
    private readonly TestDbContext dbContext;

    public CommandCH(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ExecuteAsync(HttpContext context, Command command)
    {
        dbContext.Entities.Add(new Entity { Id = command.Id, Data = command.Data, });
        return dbContext.SaveChangesAsync();
    }
}
