using Hesabdari_Core.ServiceContracts.Storage;
using Microsoft.AspNetCore.Http;

namespace Hesabdari_Core.Services.Storage;

public class VideoStorageService : IVideoStorageService
{
    private readonly string _videoFolder;

    public VideoStorageService()
    {
        _videoFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");
    }

    public async Task<string> SaveVideoAsync(IFormFile video)
    {
        if (video == null || video.Length == 0)
            throw new ArgumentException("ویدئو معتبر نیست.");

        var allowedExtensions = new[] { ".mp4" };
        var fileExtension = Path.GetExtension(video.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("فرمت ویدئو معتبر نیست. فقط MP4 مجاز است.");

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_videoFolder, fileName);

        if (!Directory.Exists(_videoFolder))
            Directory.CreateDirectory(_videoFolder);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await video.CopyToAsync(stream);

        return $"/videos/{fileName}";
    }

    public Task DeleteOldVideosAsync(params string?[] videoPaths)
    {
        foreach (var videoPath in videoPaths)
        {
            if (string.IsNullOrWhiteSpace(videoPath)) continue;

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", videoPath.TrimStart('/'));
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        return Task.CompletedTask;
    }
}