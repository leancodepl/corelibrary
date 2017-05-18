using System.IO;
using System.Threading.Tasks;

namespace LeanCode.DataStorage
{
    public interface IDataStorage
    {
        Task<string> UploadFile(string path, Stream data);
        string GeneratePathWithRandomFileName(string directory, string extension);
    }
}
