using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LeanCode.Localization.StringLocalizers;
using LeanCode.Logging;
using LeanCode.SendGrid;
using LeanCode.Test.Helpers;
using LeanCode.ViewRenderer;
using NSubstitute;
using SendGrid;
using Xunit;

namespace LeanCode.Infrastructure.SendGrid.Tests;

public class SendGridRazorClientTests
{
    private const string EmailFrom = "test@leancode.pl";

    private static readonly string EmailTo = Environment.GetEnvironmentVariable("SENDGRID_EMAILTO");

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

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
                var cultureName = Equals(culture, CultureInfo.InvariantCulture) ? "InvariantCulture" : culture.Name;
                var keyName = ci.Arg<string>();

                return $"[{cultureName}] {keyName}";
            });

        client = new SendGridRazorClient(
            new SendGridClient(new() { ApiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY") ?? "unset" }),
            renderer,
            localizer,
            NullLogger<SendGridRazorClient>.Instance
        );
    }

    [SendGridFact]
    public async Task Sends_localized_email_correctly()
    {
        var msg = new SendGridLocalizedRazorMessage("pl")
            .WithSender(EmailFrom, "LeanCode Tester")
            .WithRecipient(EmailTo)
            .WithSubject("email.subject.test")
            .WithPlainTextContent(new EmailTextVM())
            .WithHtmlContent(new EmailHtmlVM())
            .WithAttachment(Convert.ToBase64String("Attachment content."u8), "Attachment.txt", "text/plain")
            .WithNoTracking();

        await client.SendEmailAsync(msg);
    }

    [SendGridFact]
    public async Task Sends_email_with_literal_subject_correctly()
    {
        var msg = new SendGridLocalizedRazorMessage("pl")
            .WithSender(EmailFrom, "LeanCode Tester")
            .WithRecipient(EmailTo)
            .WithLiteralSubject("Test email with literal subject")
            .WithPlainTextContent(new EmailTextVM())
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
            .WithPlainTextContent(new EmailTextVM())
            .WithHtmlContent(new EmailHtmlVM())
            .WithAttachment(Convert.ToBase64String("Attachment content."u8), "Attachment.txt", "text/plain")
            .WithNoTracking();

        var exception = await Assert.ThrowsAsync<SendGridException>(() => client.SendEmailAsync(msg));

        Assert.Contains(
            "The personalizations field is required and must have at least one personalization.",
            exception.ErrorMessages
        );
    }

    // ReSharper disable once UnusedMember.Local
    private sealed class EmailTextVM
    {
        public string Value { get; set; } = "Text";
    }

    // ReSharper disable once UnusedMember.Local
    private sealed class EmailHtmlVM
    {
        public string Value { get; set; } = "Html";
    }
}

internal sealed class SendGridFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = 0
) : ExternalServiceFactAttribute(sourceFilePath, sourceLineNumber)
{
    protected override string ServiceType => "sendgrid";
    protected override IReadOnlyCollection<string> RequiredEnvVariables { get; } =
    ["SENDGRID_APIKEY", "SENDGRID_EMAILTO"];
}
