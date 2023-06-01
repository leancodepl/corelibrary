namespace LeanCode.CQRS.MassTransitRelay.Tests.Integration;

/// <remarks>
/// The purpose here is rather to verify if entities are saved in the database along with published messages.
/// </remarks>
public class HandledLog
{
    public Guid CorrelationId { get; private set; }
    public string HandlerName { get; private set; }

    private HandledLog(Guid correlationId, string handlerName)
    {
        CorrelationId = correlationId;
        HandlerName = handlerName;
    }

    public static void Report(TestDbContext dbContext, Guid correlationId, string handlerName)
    {
        dbContext.HandledLog.Add(new(correlationId, handlerName));
    }
}
