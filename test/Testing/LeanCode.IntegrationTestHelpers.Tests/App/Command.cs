using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.IntegrationTestHelpers.Tests.App
{
    public class Command : IRemoteCommand
    {
        public int Id { get; set; }
        public string? Data { get; set; }
    }

    public class CommandCH : ICommandHandler<Context, Command>
    {
        private readonly TestDbContext dbContext;

        public CommandCH(TestDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task ExecuteAsync(Context context, Command command)
        {
            dbContext.Entities.Add(new Entity
            {
                Id = command.Id,
                Data = command.Data,
            });
            return dbContext.SaveChangesAsync();
        }
    }
}
