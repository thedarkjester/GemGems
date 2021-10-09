using System.IO;
using System.Threading.Tasks;

namespace GemGems.App.Generator
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string container, string contentType);
        Task<string> UploadAsync(byte[] fileStream, string fileName, string container, string contentType);
        Task<Stream> GetFileUriAsync(string fileName, string container);
    }
}