using Microsoft.AspNetCore.Http;

namespace Hesabdari_Core.ServiceContracts.Storage;

public interface IVideoStorageService
{
    Task<string> SaveVideoAsync(IFormFile video);

    Task DeleteOldVideosAsync(params string?[] videoPaths);
}