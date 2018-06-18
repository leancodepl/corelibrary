using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ViewRenderer;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketGenerator : IPdfGenerator
    {
        private const string Endpoint = "https://api.html2pdfrocket.com/pdf";

        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PdfRocketGenerator>();

        private readonly PdfRocketConfiguration config;
        private readonly IViewRenderer viewRenderer;

        private readonly HttpClient client;

        public PdfRocketGenerator(
            PdfRocketConfiguration config,
            IViewRenderer viewRenderer,
            HttpClient client)
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
        {
            var html = await viewRenderer.RenderToString(templateName, model);
            logger.Debug("Generating PDF from template {TemplateName}", templateName);
            return await GenerateFromHtml(html);
        }

        public Task<Stream> GenerateFromUrl(string url)
        {
            logger.Debug("Generating PDF from URL {@URL}", url);
            return Generate(url);
        }

        private async Task<Stream> Generate(string source)
        {
            using (var content = GetContent(source))
            {
                var response = await client.PostAsync(Endpoint, content);
                var result = await response.Content.ReadAsStreamAsync();

                logger.Information("PDF generated");

                return result;
            }
        }

        private HttpContent GetContent(string source)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(config.ApiKey), "apiKey");
            content.Add(new StringContent(source), "value");
            return content;
        }
    }
}
