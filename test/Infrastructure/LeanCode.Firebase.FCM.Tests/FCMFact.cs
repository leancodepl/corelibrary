using System.Runtime.CompilerServices;
using LeanCode.Test.Helpers;

namespace LeanCode.Firebase.FCM.Tests;

public sealed class FCMFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = 0
) : ExternalServiceFactAttribute(sourceFilePath, sourceLineNumber)
{
    protected override string ServiceType => "fcm";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } = ["FCM_KEY", "FCM_TOKEN"];
}
