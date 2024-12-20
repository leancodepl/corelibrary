using LeanCode.Test.Helpers;

namespace LeanCode.Firebase.FCM.Tests;

public sealed class FCMFactAttribute : ExternalServiceFactAttribute
{
    protected override string ServiceType => "fcm";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } = ["FCM_KEY", "FCM_TOKEN"];
}
