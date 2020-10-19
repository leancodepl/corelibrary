using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LeanCode.ViewRenderer;

namespace LeanCode.PdfRocket
{
    public class PdfRocketGenerator
    {
        public const string ApiUrl = "https://api.html2pdfrocket.com/";
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PdfRocketGenerator>();

        private readonly PdfRocketConfiguration config;
        private readonly IViewRenderer viewRenderer;
        private readonly HttpClient client;

        public PdfRocketGenerator(PdfRocketConfiguration config, IViewRenderer viewRenderer, HttpClient client)
        {
            this.config = config;
            this.viewRenderer = viewRenderer;
            this.client = client;
        }

        public virtual Task<Stream> GenerateFromHtmlAsync(
            string html,
            PdfOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            logger.Debug("Generating PDF from supplied HTML document");

            return GenerateAsync(html, options, cancellationToken);
        }

        public virtual async Task<Stream> GenerateFromTemplateAsync<TModel>(
            string templateName,
            TModel model,
            PdfOptions? options = null,
            CancellationToken cancellationToken = default)
            where TModel : notnull
        {
            var html = await viewRenderer.RenderToStringAsync(templateName, model, cancellationToken);

            logger.Debug("Generating PDF from template {TemplateName}", templateName);

            return await GenerateFromHtmlAsync(html, options, cancellationToken);
        }

        public virtual Task<Stream> GenerateFromUrlAsync(
            string url,
            PdfOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            logger.Debug("Generating PDF from URL {@URL}", url);

            return GenerateAsync(url, options, cancellationToken);
        }

        private async Task<Stream> GenerateAsync(string source, PdfOptions? options, CancellationToken cancellationToken)
        {
            using var content = GetContent(source, options);
            using var request = new HttpRequestMessage(HttpMethod.Post, "pdf")
            {
                Content = content,
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = new MemoryStream();
            await response.Content.CopyToAsync(result, cancellationToken);
            result.Position = 0;

            logger.Information("PDF generated");

            return result;
        }

        private HttpContent GetContent(string source, PdfOptions? options)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(config.ApiKey), "apiKey" },
                { new StringContent(source), "value" },
            };

            if (options != null)
            {
                foreach (var prop in options.GetType().GetProperties())
                {
                    var val = prop.GetValue(options);
                    if (val != null)
                    {
                        content.Add(new StringContent(val.ToString()!), prop.Name);
                    }
                }
            }

            return content;
        }
    }
}
