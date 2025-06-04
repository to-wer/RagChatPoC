using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using RagChatPoC.Api.Services.Interfaces;
using RagChatPoC.Api.Utils;

namespace RagChatPoC.Api.Controllers;

/// <summary>
/// API endpoint for managing documents.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController(
    IFileProcessingService fileProcessingService,
    IDocumentService documentService) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of all available documents.
    /// </summary>
    /// <returns>A JSON array of document names.</returns>
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await documentService.GetAllDocuments();
        return Ok(documents);
    }

    /// <summary>
    /// Uploads a file to the server and processes its contents.
    /// </summary>
    /// <param name="file">The file to be uploaded.</param>
    /// <returns>A success message if the file is successfully processed, or an error message if there was an issue with the upload or processing.</returns>
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

    /// <summary>
    /// Uploads a zip file containing multiple documents to the server and processes each document's contents.
    /// </summary>
    /// <param name="zipFile">The zip file to be uploaded.</param>
    /// <returns>A success message if all files are successfully processed, or an error message if there was an issue with the upload or processing.</returns>
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

    /// <summary>
    /// Deletes a document from the server.
    /// </summary>
    /// <param name="fileName">The name of the document to be deleted.</param>
    /// <returns>A success message if the file is successfully deleted, or an error message if there was an issue with the deletion.</returns>
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteDocument(string fileName)
    {
        await documentService.DeleteDocument(fileName);
        return Ok("File deleted.");
    }
}