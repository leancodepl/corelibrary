using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using LeanCode.SendGrid;
using LeanCode.ViewRenderer;
using NSubstitute;
using SendGrid;
using Xunit;

namespace LeanCode.Infrastructure.SendGrid.Tests;

public class SendGridRazorClientTests
{
    private const string EmailFrom = "test@leancode.pl";

    private static readonly string EmailTo = Environment.GetEnvironmentVariable("SENDGRID_EMAILTO");

    private static readonly SendGridClientOptions Options = new SendGridClientOptions
    {
        ApiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY"),
    };

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true, };

    private readonly SendGridRazorClient client;

    public SendGridRazorClientTests()
    {
        var renderer = Substitute.For<IViewRenderer>();

        renderer
            .RenderToStringAsync(default, default)
            .ReturnsForAnyArgs(ci =>
            {
                var templateName = ci.Arg<string>();
                var model = ci.Arg<object>();

                var rendered = $"{templateName}:\n{JsonSerializer.Serialize(model, model.GetType(), JsonOptions)}";

                return templateName.Contains(".txt", StringComparison.Ordinal)
                    ? rendered
                    : string.Join(null, rendered.Split('\n').Select(l => $"<div>{l}</div>"));
            });

        var localizer = Substitute.For<IStringLocalizer>();

        localizer[default, default]
            .ReturnsForAnyArgs(ci =>
            {
                var culture = ci.Arg<CultureInfo>();
                var cultureName = culture == CultureInfo.InvariantCulture ? "InvariantCulture" : culture.Name;
                var keyName = ci.Arg<string>();

                return $"[{cultureName}] {keyName}";
            });

        client = new SendGridRazorClient(new SendGridClient(Options), renderer, localizer);
    }

    internal sealed class SendGridFactAttribute : FactAttribute
    {
        public SendGridFactAttribute()
        {
            if (string.IsNullOrEmpty(Options.ApiKey))
            {
                Skip = "API key not set";
            }
            else if (string.IsNullOrEmpty(EmailTo))
            {
                Skip = "No recipient provided";
            }
        }
    }

    [SendGridFact]
    public async Task Sends_email_correctly()
    {
        var msg = new SendGridLocalizedRazorMessage("pl")
            .WithSender(EmailFrom, "LeanCode Tester")
            .WithRecipient(EmailTo)
            .WithSubject("email.subject.test")
            .WithPlainTextContent(new EmailTextVM { })
            .WithHtmlContent(new EmailHtmlVM { })
            .WithAttachment(
                Convert.ToBase64String(Encoding.UTF8.GetBytes("Attachment content.")),
                "Attachment.txt",
                "text/plain"
            )
            .WithNoTracking();

        await client.SendEmailAsync(msg);
    }

    [SendGridFact]
    public async Task Throws_when_sending_failed()
    {
        var msg = new SendGridLocalizedRazorMessage("pl")
            .WithSender(EmailFrom, "LeanCode Tester")
            // .WithRecipient(EmailTo) omitted on purpose to cause a failure
            .WithSubject("email.subject.test")
            .WithPlainTextContent(new EmailTextVM { })
            .WithHtmlContent(new EmailHtmlVM { })
            .WithAttachment(
                Convert.ToBase64String(Encoding.UTF8.GetBytes("Attachment content.")),
                "Attachment.txt",
                "text/plain"
            )
            .WithNoTracking();

        var exception = await Assert.ThrowsAsync<SendGridException>(() => client.SendEmailAsync(msg));

        Assert.Contains(
            "The personalizations field is required and must have at least one personalization.",
            exception.ErrorMessages
        );
    }

    private sealed class EmailTextVM
    {
        public string Value { get; set; } = "Text";
    }

    private sealed class EmailHtmlVM
    {
        public string Value { get; set; } = "Html";
    }
}
