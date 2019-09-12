using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ViewRenderer;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketGenerator : IPdfGenerator
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PdfRocketGenerator>();

        private readonly PdfRocketConfiguration config;
        private readonly IViewRenderer viewRenderer;
        private readonly PdfRocketHttpClient client;

        public PdfRocketGenerator(PdfRocketConfiguration config, IViewRenderer viewRenderer, PdfRocketHttpClient client)
        {
            this.config = config;
            this.viewRenderer = viewRenderer;
            this.client = client;
        }

        public Task<Stream> GenerateFromHtml(string html)
        {
            logger.Debug("Generating PDF from supplied HTML document");

            return Generate(html);
        }

        public async Task<Stream> GenerateFromTemplate<TModel>(string templateName, TModel model)
            where TModel : notnull
        {
            var html = await viewRenderer
                .RenderToStringAsync(templateName, model)
                .ConfigureAwait(false);

            logger.Debug("Generating PDF from template {TemplateName}", templateName);

            return await GenerateFromHtml(html).ConfigureAwait(false);
        }

        public Task<Stream> GenerateFromUrl(string url)
        {
            logger.Debug("Generating PDF from URL {@URL}", url);

            return Generate(url);
        }

        private async Task<Stream> Generate(string source)
        {
            using var content = GetContent(source);

            using var response = await client.Client
                .PostAsync("pdf", content)
                .ConfigureAwait(false);

            using var result = new MemoryStream();

            await response.Content
                .CopyToAsync(result)
                .ConfigureAwait(false);

            result.Position = 0;

            logger.Information("PDF generated");

            return result;
        }

        private HttpContent GetContent(string source)
        {
            return new MultipartFormDataContent()
            {
                { new StringContent(config.ApiKey), "apiKey" },
                { new StringContent(source), "value" },
            };
        }
    }
}
