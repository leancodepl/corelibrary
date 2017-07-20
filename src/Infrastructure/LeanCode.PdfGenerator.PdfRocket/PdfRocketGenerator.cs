using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using LeanCode.ViewRenderer;

namespace LeanCode.PdfGenerator.PdfRocket
{
    public class PdfRocketGenerator : IPdfGenerator, IDisposable
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<PdfRocketGenerator>();

        private readonly PdfRocketConfiguration config;
        private readonly IViewRenderer viewRenderer;

        private readonly HttpClient client;

        public PdfRocketGenerator(PdfRocketConfiguration config, IViewRenderer viewRenderer)
        {
            this.config = config;
            this.viewRenderer = viewRenderer;
            client = new HttpClient
            {
                BaseAddress = new Uri("http://api.html2pdfrocket.com")
            };
        }

        public void Dispose()
        {
            client.Dispose();
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
                var response = await client.PostAsync("pdf", content);
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
