using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UploadsController(IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Arquivo nao informado." });
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Somente arquivos de imagem sao aceitos." });
        }

        var extension = Path.GetExtension(file.FileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension;
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadsRoot = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var subFolder = DateTime.UtcNow.ToString("yyyyMMdd");
        var targetFolder = Path.Combine(uploadsRoot, subFolder);
        Directory.CreateDirectory(targetFolder);

        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(targetFolder, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        var absoluteUrl = $"{Request.Scheme}://{Request.Host}/uploads/{subFolder}/{fileName}";
        return Ok(new UploadResponse(absoluteUrl));
    }
}

public record UploadResponse(string Url);
