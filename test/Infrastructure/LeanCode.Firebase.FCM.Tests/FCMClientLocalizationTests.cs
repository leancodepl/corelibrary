using System.Globalization;
using FirebaseAdmin.Messaging;
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
        const string Key = "https://example.com/image.jpg";
        const string Value = "https://example.com/image_en.jpg";
        stringLocalizer[Culture, Key].Returns(Value);

        var n = client.Localize(Culture).ImageUrl(new Uri(Key), []).Build();

        Assert.Equal(Value, n.ImageUrl);
    }

    [Fact]
    public void Does_not_localize_raw_url()
    {
        const string Value = "https://example.com/image.jpg";

        var n = client.Localize(Culture).RawImageUrl(new Uri(Value)).Build();

        Assert.Equal(Value, n.ImageUrl);
    }
}
