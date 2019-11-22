using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.Localization.StringLocalizers;
using LeanCode.ViewRenderer;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LeanCode.SendGrid
{
    public class SendGridRazorClient : SendGridClient
    {
        internal const string PlainTextModelSuffix = ".txt";
        internal const string HtmlModelSuffix = "";

        private const string IncorrectUsageMessage = "Use SendEmailAsync(SendGridRazorMessage, CancellationToken) overload instead.";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<SendGridRazorClient>();

        private readonly IViewRenderer renderer;
        private readonly IStringLocalizer localizer;

        public SendGridRazorClient(SendGridClientOptions options, IViewRenderer renderer, IStringLocalizer localizer)
            : base(options)
        {
            this.renderer = renderer;
            this.localizer = localizer;
        }

        public async Task<Response> SendEmailAsync(
            SendGridRazorMessage msg,
            CancellationToken cancellationToken = default)
        {
            if (msg is SendGridLocalizedRazorMessage lrmsg)
            {
                lrmsg.Subject = LocalizeSubject(lrmsg.Culture, lrmsg.Subject, lrmsg.SubjectFormatArgs);
            }

            if (msg.PlainTextContentModel is object plainTextModel)
            {
                var viewName = plainTextModel.GetType().Name;

                msg.PlainTextContent = await msg
                    .GenerateTemplateNames(viewName)
                    .SelectAsync(name => TryRenderViewAsync(name + PlainTextModelSuffix, plainTextModel))
                    .Where(view => view != null)
                    .FirstOrDefaultAsync();

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
                    .GenerateTemplateNames(viewName)
                    .SelectAsync(name => TryRenderViewAsync(name + HtmlModelSuffix, htmlModel))
                    .Where(view => view != null)
                    .FirstOrDefaultAsync();

                if (msg.HtmlContent is null)
                {
                    logger.Error("Failed to render all HTML views for {ViewName}.", viewName);

                    throw new ViewNotFoundException(viewName, "Failed to render all HTML views.");
                }
            }

            return await base.SendEmailAsync(msg, cancellationToken);
        }

        [Obsolete(IncorrectUsageMessage)]
        public new Task<Response> SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default)
        {
            return msg is SendGridRazorMessage
                ? throw new InvalidOperationException(IncorrectUsageMessage)
                : base.SendEmailAsync(msg, cancellationToken);
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
