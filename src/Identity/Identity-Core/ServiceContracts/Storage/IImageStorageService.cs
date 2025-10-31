using Microsoft.AspNetCore.Http;

namespace Hesabdari_Core.ServiceContracts.Storage;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(IFormFile image);

    Task DeleteOldImagesAsync(params string?[] imagePaths);

    Task<string> ConvertToWebpAsync(IFormFile image);
}