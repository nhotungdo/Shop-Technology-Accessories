using Microsoft.AspNetCore.Http;

namespace ShopTechnology.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "uploads");
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
        Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string folder = "uploads");
        bool IsValidFile(IFormFile file);
        Task<string> ResizeImageAsync(string filePath, int width, int height);
    }
}
