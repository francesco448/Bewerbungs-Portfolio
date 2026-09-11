using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BewerbungsSeite.Models.SiteContent;
using BewerbungsSeite.Services;

namespace BewerbungsSeite.Controllers.Api.V1;

/// <summary>
/// Schreibzugriff auf den Seiteninhalt. Nur fuer angemeldete Administratoren:
/// die Konvention AuthorizeFolder("/Admin") gilt nur fuer Razor Pages, deshalb
/// steht das [Authorize] hier ausdruecklich am Controller.
/// </summary>
[Authorize]
[AutoValidateAntiforgeryToken]
[ApiController]
[Route("api/v1/[controller]")]
public class PageContentController(
    IPageContentService pageContentService,
    IWebHostEnvironment environment,
    ILogger<PageContentController> logger) : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly string[] AllowedDocumentExtensions = [".pdf"];

    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxDocumentBytes = 20 * 1024 * 1024;

    /// <summary>Schreibt den kompletten Seiteninhalt zurueck.</summary>
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] PageContent content)
    {
        if (content is null)
        {
            return BadRequest(new { error = "Kein Inhalt übermittelt." });
        }

        await pageContentService.SaveAsync(content, HttpContext.RequestAborted);
        return Ok(new { success = true, savedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Nimmt ein Bild entgegen, legt es unter wwwroot/uploads/images ab und
    /// liefert den Pfad zurueck, der anschliessend im JSON gespeichert wird.
    /// </summary>
    [HttpPost("images")]
    [RequestSizeLimit(MaxImageBytes)]
    public Task<IActionResult> UploadImage(IFormFile? file) =>
        StoreAsync(file, "images", AllowedImageExtensions, MaxImageBytes,
            requiredContentTypePrefix: "image/",
            wrongTypeMessage: "Erlaubt sind JPG, PNG, WEBP und GIF.",
            tooLargeMessage: "Die Datei ist grösser als 5 MB.");

    /// <summary>
    /// Nimmt ein Dokument entgegen (PDF), legt es unter wwwroot/uploads/documents
    /// ab und liefert den Pfad zurueck.
    /// </summary>
    [HttpPost("documents")]
    [RequestSizeLimit(MaxDocumentBytes)]
    public Task<IActionResult> UploadDocument(IFormFile? file) =>
        StoreAsync(file, "documents", AllowedDocumentExtensions, MaxDocumentBytes,
            requiredContentTypePrefix: null,
            wrongTypeMessage: "Erlaubt sind nur PDF-Dateien.",
            tooLargeMessage: "Die Datei ist grösser als 20 MB.");

    private async Task<IActionResult> StoreAsync(
        IFormFile? file,
        string folder,
        string[] allowedExtensions,
        long maxBytes,
        string? requiredContentTypePrefix,
        string wrongTypeMessage,
        string tooLargeMessage)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "Keine Datei übermittelt." });
        }

        if (file.Length > maxBytes)
        {
            return BadRequest(new { error = tooLargeMessage });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = wrongTypeMessage });
        }

        if (requiredContentTypePrefix is not null &&
            !file.ContentType.StartsWith(requiredContentTypePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = wrongTypeMessage });
        }

        // Der Dateiname wird selbst vergeben; der Name aus dem Browser wird nie uebernommen.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(environment.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadDirectory);

        var targetPath = Path.Combine(uploadDirectory, fileName);
        await using (var stream = System.IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream, HttpContext.RequestAborted);
        }

        logger.LogInformation("Datei hochgeladen: {Folder}/{FileName}", folder, fileName);

        return Ok(new { url = $"/uploads/{folder}/{fileName}" });
    }
}
