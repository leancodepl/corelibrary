using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using LeanCode.ViewRenderer;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LeanCode.SendGrid
{
    public class SendGridRazorClient
    {
        private const string PlainTextModelSuffix = ".txt";
        private const string HtmlModelSuffix = "";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SendGridRazorClient>();

        private readonly SendGridClient client;
        private readonly IViewRenderer renderer;
        private readonly IStringLocalizer localizer;

        public SendGridRazorClient(SendGridClient client, IViewRenderer renderer, IStringLocalizer localizer)
        {
            this.client = client;
            this.renderer = renderer;
            this.localizer = localizer;
        }

        public async Task SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
        {
            if (msg is SendGridRazorMessage rmsg)
            {
                await RenderMessageAsync(rmsg);
            }

            var response = await client.SendEmailAsync(msg, cancellationToken);

            var statusCode = response.StatusCode;

            if (statusCode >= HttpStatusCode.BadRequest)
            {
                await using var stream = await response.Body.ReadAsStreamAsync();

                try
                {
                    var body = await JsonSerializer.DeserializeAsync<SendGridResponse?>(stream, SerializerOptions);

                    throw new SendGridException(statusCode, body?.Errors);
                }
                catch (JsonException)
                {
                    throw new SendGridException(statusCode, null);
                }
            }
        }

        protected virtual async Task RenderMessageAsync(SendGridRazorMessage msg)
        {
            if (msg is SendGridLocalizedRazorMessage lrmsg)
            {
                lrmsg.Subject = LocalizeSubject(lrmsg.Culture, lrmsg.Subject, lrmsg.SubjectFormatArgs);
            }

            if (msg.PlainTextContentModel is object plainTextModel)
            {
                var viewName = plainTextModel.GetType().Name;

                msg.PlainTextContent = await msg
                    .GetTemplateNames(viewName)
                    .SelectAsync(name => TryRenderViewAsync(name + PlainTextModelSuffix, plainTextModel))
                    .FirstOrDefaultAsync(view => view != null);

                if (msg.PlainTextContent is null)
                {
                    logger.Error("Failed to render all plain text views for {ViewName}.", viewName);

                    throw new ViewNotFoundException(viewName, "Failed to render all plain text views.");
                }
            }

            if (msg.HtmlContentModel is object htmlModel)
            {
                var viewName = htmlModel.GetType().Name;

                msg.HtmlContent = await msg
                    .GetTemplateNames(viewName)
                    .SelectAsync(name => TryRenderViewAsync(name + HtmlModelSuffix, htmlModel))
                    .FirstOrDefaultAsync(view => view != null);

                if (msg.HtmlContent is null)
                {
                    logger.Error("Failed to render all HTML views for {ViewName}.", viewName);

                    throw new ViewNotFoundException(viewName, "Failed to render all HTML views.");
                }
            }
        }

        private async Task<string?> TryRenderViewAsync(string viewName, object model)
        {
            try
            {
                return await renderer.RenderToStringAsync(viewName, model);
            }
            catch (ViewNotFoundException ex)
            {
                logger.Debug(ex, "Cannot locate view {ViewName}", viewName);

                return null;
            }
        }

        private string LocalizeSubject(CultureInfo culture, string subjectKey, object[]? formatArgs)
        {
            return formatArgs is object[] args
                ? localizer.Format(culture, subjectKey, args)
                : localizer[culture, subjectKey];
        }
    }
}
