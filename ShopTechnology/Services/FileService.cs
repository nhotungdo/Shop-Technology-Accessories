// using Cloudinary;
// using CloudinaryDotNet;
// using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace ShopTechnology.Services
{
    public class FileService : IFileService
    {
        // private readonly Cloudinary _cloudinary;

        public FileService(IConfiguration configuration)
        {
            // var cloudName = configuration["CloudinarySettings:CloudName"];
            // var apiKey = configuration["CloudinarySettings:ApiKey"];
            // var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            // var account = new Account(cloudName, apiKey, apiSecret);
            // _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            if (!await ValidateImageAsync(file))
                return string.Empty;

            // Temporary implementation - return placeholder URL
            return await Task.FromResult($"/uploads/{folder}/{Guid.NewGuid()}.jpg");
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            // Temporary implementation - always return true
            return await Task.FromResult(true);
        }

        public async Task<string> UploadProductImageAsync(IFormFile file)
        {
            return await UploadImageAsync(file, "products");
        }

        public async Task<string> UploadBannerImageAsync(IFormFile file)
        {
            return await UploadImageAsync(file, "banners");
        }

        public async Task<string> UploadAvatarAsync(IFormFile file)
        {
            return await UploadImageAsync(file, "avatars");
        }

        public async Task<string> UploadReviewImageAsync(IFormFile file)
        {
            return await UploadImageAsync(file, "reviews");
        }

        public async Task<bool> ValidateImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return false;

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        public async Task<string> GetImageUrlAsync(string fileName, string folder)
        {
            // This would be used for local file storage
            // For now, return empty string as we're using Cloudinary
            return await Task.FromResult(string.Empty);
        }

                // private string ExtractPublicIdFromUrl(string url)
        // {
        //     try
        //     {
        //         var uri = new Uri(url);
        //         var path = uri.AbsolutePath;
        //         var segments = path.Split('/');
        //         
        //         // Find the upload segment and get the public ID
        //         for (int i = 0; i < segments.Length - 1; i++)
        //         {
        //             if (segments[i] == "upload")
        //             {
        //                 var publicId = string.Join("/", segments.Skip(i + 2));
        //                 return publicId.Replace(".jpg", "").Replace(".png", "").Replace(".gif", "").Replace(".webp", "");
        //             }
        //         }
        //         
        //         return string.Empty;
        //     }
        //     catch
        //     {
        //         return string.Empty;
        //     }
        // }
    }
}
