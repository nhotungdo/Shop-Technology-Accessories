namespace ShopTechnology.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<string> UploadProductImageAsync(IFormFile file);
        Task<string> UploadBannerImageAsync(IFormFile file);
        Task<string> UploadAvatarAsync(IFormFile file);
        Task<string> UploadReviewImageAsync(IFormFile file);
        Task<bool> ValidateImageAsync(IFormFile file);
        Task<string> GetImageUrlAsync(string fileName, string folder);
    }
}
