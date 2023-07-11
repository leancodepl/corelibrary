using Microsoft.Extensions.Time.Testing;

namespace LeanCode.TimeProvider.TestHelpers;

public static class TestTimeProvider
{
    public static FakeTimeProvider ActivateFake(DateTime epoch, TimeZoneInfo? localTimeZone = null) =>
        ActivateFake(new DateTimeOffset(epoch, TimeSpan.Zero), localTimeZone);

    public static FakeTimeProvider ActivateFake(DateTimeOffset epoch, TimeZoneInfo? localTimeZone = null)
    {
        var provider = new FakeTimeProvider(epoch.ToUniversalTime());

        if (localTimeZone is not null)
        {
            provider.SetLocalTimeZone(localTimeZone);
        }

        AsyncLocalTimeProvider.Activate(provider);

        return provider;
    }
}
