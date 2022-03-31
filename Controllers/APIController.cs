using Microsoft.AspNetCore.Mvc;
using VoiceGradeApi.Services;

namespace VoiceGradeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private string _guid;
    private readonly string _directoryPath;
    private ProcessingService? _processingService;

    public FileUploadController()
    {
        _guid = Guid.NewGuid().ToString();
        _directoryPath = (Directory.GetCurrentDirectory() + ($@"\UploadedFiles\{_guid}"));
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadFiles(IFormFile file1, IFormFile file2)
    {
        if (!Directory.Exists(_directoryPath)) Directory.CreateDirectory(_directoryPath);
        List<string> downloadedFiles = new List<string>();

        foreach (IFormFile file in new List<IFormFile> {file1, file2})
        {
            var originalName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_directoryPath, originalName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);
            downloadedFiles.Add(filePath);
        }

        Ok("Successfully uploaded");
        _processingService = HttpContext.RequestServices.GetService<ProcessingService>();
        var res = _processingService?.GetResultedFile(downloadedFiles);
        byte[] bytes = await System.IO.File.ReadAllBytesAsync(res ?? string.Empty);
        return File(bytes, "application/json", res);
    }
}