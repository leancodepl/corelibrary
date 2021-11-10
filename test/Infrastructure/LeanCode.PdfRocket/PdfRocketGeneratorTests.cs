using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LeanCode.PdfRocket.Tests
{
    public class PdfRocketGeneratorTests
    {
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("PDF_ROCKET_API_KEY") ?? "";

        internal sealed class PdfRocketFactAttribute : FactAttribute
        {
            public PdfRocketFactAttribute()
            {
                if (string.IsNullOrEmpty(ApiKey))
                {
                    Skip = "PdfRocket tests require setting PDF_ROCKET_API_KEY variable";
                }
            }
        }

        private readonly PdfRocketGenerator generator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2000", Justification = "References don't go out of scope.")]
        public PdfRocketGeneratorTests()
        {
            var config = new PdfRocketConfiguration
            {
                ApiKey = ApiKey,
            };

            generator = new PdfRocketGenerator(config, null!, new HttpClient
            {
                BaseAddress = new Uri(PdfRocketGenerator.ApiUrl),
            });
        }

        [PdfRocketFact]
        public async Task Converts_pdf_correctly()
        {
            var html = @"
            <html>
                <body>
                    Some pdf content
                    <img src='https://cataas.com/cat' />
                </body>
            </html>";

            var options = new PdfOptions
            {
                PageWidth = 200,
                PageHeight = 400,
            };

            using var stream = await generator.GenerateFromHtmlAsync(html, options);

            using var file = File.OpenWrite($"{Guid.NewGuid()}.pdf");
            await stream.CopyToAsync(file);
        }
    }
}
