using System.Globalization;
using FirebaseAdmin.Messaging;
using LeanCode.Firebase;
using LeanCode.Localization.StringLocalizers;
using NSubstitute;
using Xunit;

namespace LeanCode.Firebase.FCM.Tests;

public class FCMClientLocalizationTests
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pl");
    private static readonly FirebaseMessaging Messaging = FirebaseMessaging.GetMessaging(
        FirebaseConfiguration.Prepare(null, "[NULL]")
    );

    private readonly IStringLocalizer stringLocalizer;
    private readonly FCMClient<Guid> client;

    public FCMClientLocalizationTests()
    {
        stringLocalizer = Substitute.For<IStringLocalizer>();
        client = new FCMClient<Guid>(Messaging, Substitute.For<IPushNotificationTokenStore<Guid>>(), stringLocalizer);
    }

    [Fact]
    public void Localizes_title_correctly()
    {
        const string Key = "TITLE";
        const string Value = "formatted title";
        stringLocalizer[Culture, Key].Returns(Value);

        var n = client.Localize(Culture).Title(Key).Build();

        Assert.Equal(Value, n.Title);
    }

    [Fact]
    public void Localizes_body_correctly()
    {
        const string Key = "BODY";
        const string Value = "formatted body";
        stringLocalizer[Culture, Key].Returns(Value);

        var n = client.Localize(Culture).Body(Key).Build();

        Assert.Equal(Value, n.Body);
    }

    [Fact]
    public void Localizes_ImageUrl_correctly()
    {
        const string Key = "URL";
        const string Value = "formatted image url";
        stringLocalizer[Culture, Key].Returns(Value);

        var n = client.Localize(Culture).ImageUrl(Key, System.Array.Empty<object>()).Build();

        Assert.Equal(Value, n.ImageUrl);
    }

    [Fact]
    public void Does_not_localize_raw_url()
    {
        const string Value = "raw image url";

        var n = client.Localize(Culture).RawImageUrl(Value).Build();

        Assert.Equal(Value, n.ImageUrl);
    }
}
