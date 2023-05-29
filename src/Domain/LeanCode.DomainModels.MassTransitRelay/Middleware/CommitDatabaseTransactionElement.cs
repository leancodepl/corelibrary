using LeanCode.Pipelines;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay.Middleware;

public class CommitDatabaseTransactionElement<TContext, TInput, TOutput, TDbContext>
    : IPipelineElement<TContext, TInput, TOutput>
    where TContext : notnull, IPipelineContext
    where TDbContext : DbContext
{
    private readonly TDbContext dbContext;

    public CommitDatabaseTransactionElement(TDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<TOutput> ExecuteAsync(TContext ctx, TInput input, Func<TContext, TInput, Task<TOutput>> next)
    {
        var result = await next(ctx, input);
        await dbContext.SaveChangesAsync(ctx.CancellationToken);
        return result;
    }
}

public static class CommitDatabaseTransactionElementExtensions
{
    public static CommitDatabaseTransactionElementBuilder<TContext, TInput, TOutput> CommitDatabaseTransaction<
        TContext,
        TInput,
        TOutput
    >(this PipelineBuilder<TContext, TInput, TOutput> builder)
        where TContext : notnull, IPipelineContext
    {
        return new CommitDatabaseTransactionElementBuilder<TContext, TInput, TOutput>(builder);
    }

    // This way we can spare generic types repetition for callers
    public class CommitDatabaseTransactionElementBuilder<TContext, TInput, TOutput>
        where TContext : notnull, IPipelineContext
    {
        private readonly PipelineBuilder<TContext, TInput, TOutput> builder;

        public CommitDatabaseTransactionElementBuilder(PipelineBuilder<TContext, TInput, TOutput> builder)
        {
            this.builder = builder;
        }

        public PipelineBuilder<TContext, TInput, TOutput> Using<TDbContext>()
            where TDbContext : DbContext
        {
            return builder.Use<CommitDatabaseTransactionElement<TContext, TInput, TOutput, TDbContext>>();
        }
    }
}
