using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LeanCode.EmailSender.SendGrid;
using LeanCode.Localization.StringLocalizers;
using LeanCode.ViewRenderer;
using NSubstitute;
using Xunit;

namespace LeanCode.Infrastructure.EmailSender.SendGrid.Tests
{
    public class SendGridClientTests
    {
        private static readonly string SendGridKey = string.Empty;

        private static readonly SendGridConfiguration Config = new SendGridConfiguration
        {
            ApiKey = SendGridKey,
        };

        private readonly SendGridClient client;

        public SendGridClientTests()
        {
            IViewRenderer renderer = Substitute.For<IViewRenderer>();

            renderer.RenderToStringAsync(null, null!).ReturnsForAnyArgs("rendered string");
            renderer.RenderToStreamAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Do<Stream>(s => new StreamWriter(s).WriteLine("Write line")));
            IStringLocalizer localizer = new MockStringLocalizer();

            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.sendgrid.com/v3/"),
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.ApiKey);

            client = new SendGridClient(
                localizer,
                renderer,
                new SendGridHttpClient(httpClient));
        }

        [SuppressMessage("?", "xUnit1004", Justification = "Requires custom data.")]
        // [Fact(Skip = "SendGrid credentials required")]
        [Fact]
        public async Task Sends_email_correctly()
        {
            await client.New()
                .From("tester@leancode.pl", "Tester")
                .To("lukasz.g@leancode.pl", "Test")
                .WithSubject("TEST")
                .WithTextContent(new Body())
                .SendAsync();
        }

        private class MockStringLocalizer : IStringLocalizer
        {
            public string this[CultureInfo culture, string name] => name;
        }

        private class Body
        {
            public string Value { get; set; } = "Value";
        }
    }
}
