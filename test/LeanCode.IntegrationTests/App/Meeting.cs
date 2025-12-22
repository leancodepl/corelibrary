using LeanCode.DomainModels.Ids;
using LeanCode.DomainModels.Model;

namespace LeanCode.IntegrationTests.App;

[TypedId(TypedIdFormat.PrefixedGuid)]
public readonly partial record struct MeetingId;

public class Meeting
{
    public MeetingId Id { get; set; }
    public string Name { get; set; } = default!;
    public TimestampTz StartTime { get; set; } = default!;
}
