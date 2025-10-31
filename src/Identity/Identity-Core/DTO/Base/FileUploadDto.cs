using Microsoft.AspNetCore.Http;

namespace Identity_Core.DTO.Base;

public class FileUploadDto
{
    public int Id { get; set; }

    public required IFormFile Image { get; set; }
}