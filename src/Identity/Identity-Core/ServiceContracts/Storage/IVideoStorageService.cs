using Microsoft.AspNetCore.Http;

namespace Identity_Core.ServiceContracts.Storage;

public interface IVideoStorageService
{
    Task<string> SaveVideoAsync(IFormFile video);

    Task DeleteOldVideosAsync(params string?[] videoPaths);
}