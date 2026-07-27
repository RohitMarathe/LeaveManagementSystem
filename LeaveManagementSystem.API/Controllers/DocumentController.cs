using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Domain.Dtos.Document;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentController : BaseController
{
    private readonly IDocumentService _service;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentService service,
        ILogger<DocumentController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Upload Employee Document
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Document upload request received. EmployeeId: {EmployeeId}, FileName: {FileName}",
            request.EmployeeId,
            request.FileName);

        var response =
            await _service.UploadAsync(
                request,
                cancellationToken);

        _logger.LogInformation(
            "Document uploaded successfully. EmployeeId: {EmployeeId}",
            request.EmployeeId);

        return Ok(response);
    }

    /// <summary>
    /// Download Document
    /// </summary>
    [HttpGet("{documentId:int}")]
    public async Task<IActionResult> Download(
        int documentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Download request received. DocumentId: {DocumentId}",
            documentId);

        var stream =
            await _service.DownloadAsync(
                documentId,
                cancellationToken);

        _logger.LogInformation(
            "Document downloaded successfully. DocumentId: {DocumentId}",
            documentId);

        return File(
            stream,
            "application/octet-stream");
    }

    /// <summary>
    /// Delete Document
    /// </summary>
    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete(
        int documentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Delete request received. DocumentId: {DocumentId}",
            documentId);

        await _service.DeleteAsync(
            documentId,
            cancellationToken);

        _logger.LogInformation(
            "Document deleted successfully. DocumentId: {DocumentId}",
            documentId);

        return NoContent();
    }
}