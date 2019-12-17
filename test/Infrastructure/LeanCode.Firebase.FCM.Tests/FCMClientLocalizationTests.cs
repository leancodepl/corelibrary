using System.Globalization;
using FirebaseAdmin.Messaging;
using LeanCode.Firebase;
using LeanCode.Localization.StringLocalizers;
using NSubstitute;
using Xunit;

namespace LeanCode.Firebase.FCM.Tests
{
    public class FCMClientLocalizationTests
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pl");
        private static readonly FirebaseMessaging Messaging;

        private readonly IStringLocalizer stringLocalizer;
        private readonly FCMClient client;

        static FCMClientLocalizationTests()
        {
            var app = FirebaseConfiguration.Prepare(null, "[NULL]");
            Messaging = FirebaseMessaging.GetMessaging(app);
        }

        public FCMClientLocalizationTests()
        {
            stringLocalizer = Substitute.For<IStringLocalizer>();
            client = new FCMClient(Messaging, Substitute.For<IPushNotificationTokenStore>(), stringLocalizer);
        }

        [Fact]
        public void Localizes_title_correctly()
        {
            const string Key = "TITLE";
            const string Value = "formatted title";
            stringLocalizer[Culture, Key].Returns(Value);

            var n = client.Localize(Culture)
                .Title(Key)
                .Build();

            Assert.Equal(Value, n.Title);
        }

        [Fact]
        public void Localizes_body_correctly()
        {
            const string Key = "BODY";
            const string Value = "formatted body";
            stringLocalizer[Culture, Key].Returns(Value);

            var n = client.Localize(Culture)
                .Body(Key)
                .Build();

            Assert.Equal(Value, n.Body);
        }

        [Fact]
        public void Localizes_ImageUrl_correctly()
        {
            const string Key = "URL";
            const string Value = "formatted image url";
            stringLocalizer[Culture, Key].Returns(Value);

            var n = client.Localize(Culture)
                .ImageUrl(Key, new object[0])
                .Build();

            Assert.Equal(Value, n.ImageUrl);
        }

        [Fact]
        public void Does_not_localize_raw_url()
        {
            const string Value = "raw image url";

            var n = client.Localize(Culture)
                .RawImageUrl(Value)
                .Build();

            Assert.Equal(Value, n.ImageUrl);
        }
    }
}
