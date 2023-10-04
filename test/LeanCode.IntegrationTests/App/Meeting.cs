using LeanCode.DomainModels.Model;

namespace LeanCode.IntegrationTests.App;

public class Meeting
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public TimestampTz StartTime { get; set; } = default!;
}
