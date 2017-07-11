using System.IO;
using System.Threading.Tasks;

namespace LeanCode.PdfGenerator
{
    public interface IPdfGenerator
    {
        Task<Stream> GenerateFromHtml(string html);
        Task<Stream> GenerateFromUrl(string url);
        Task<Stream> GenerateFromTemplate<TModel>(string templateName, TModel model);
    }
}
