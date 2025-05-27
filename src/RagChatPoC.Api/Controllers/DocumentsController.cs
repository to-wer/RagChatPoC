using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Api.Utils;

namespace RagChatPoC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController(IFileProcessingService fileProcessingService,
    IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await documentService.GetAllDocuments();
        return Ok(documents);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
        
        await using var fileStream = file.OpenReadStream();
        
        if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileStream.Seek(0, SeekOrigin.Begin);

            var rawText = PdfHelper.ExtractTextFromPdf(fileStream);
            var cleanText = TextSanitizer.CleanTextForPostgres(rawText);
            await fileProcessingService.ProcessTextAsync(file.FileName, cleanText);
        }
        else 
        {
            var text = await new StreamReader(fileStream).ReadToEndAsync();
            await fileProcessingService.ProcessTextAsync(file.FileName, text);
        }

        return Ok("File indexed.");
    }
    
    [HttpPost("upload-zip")]
    public async Task<IActionResult> UploadZip(IFormFile? zipFile)
    {
        if (zipFile == null || zipFile.Length == 0) return BadRequest("No file uploaded.");

        await using var stream = zipFile.OpenReadStream();
        using var archive = new ZipArchive(stream);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith('/')) continue;

            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);

            var text = await reader.ReadToEndAsync();

            await fileProcessingService.ProcessTextAsync(entry.FullName, text);
        }

        return Ok("Files indexed.");
    }
    
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteDocument(string fileName)
    {
        await documentService.DeleteDocument(fileName);
        return Ok("File deleted.");
    }
}