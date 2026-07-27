using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Domain.Dtos.Document;
using LeaveManagementSystem.Domain.Entities;
using LeaveManagementSystem.Domain.Models.Document;

namespace LeaveManagementSystem.Application.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IStorageService _storageService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IStorageService storageService,
        IEmployeeRepository employeeRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _storageService = storageService;
        _employeeRepository = employeeRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentResponse> UploadAsync(
    UploadDocumentRequest request,
    CancellationToken cancellationToken)
    {
        var employee =
            await _employeeRepository.GetByIdAsync(
                request.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            throw new Exception("Employee not found.");
        }

        var uploadResult =
            await _storageService.UploadFileAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                request.FileSize,
                "Documents",
                cancellationToken);

        var document = new Document
        {
            EmployeeId = employee.Id,

            FileName = request.FileName,

            BlobName = uploadResult.BlobName,

            ContentType = uploadResult.ContentType,

            FileSize = uploadResult.FileSize,

            UploadedOn = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(
            document,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentResponse
        {
            Id = document.Id,

            FileName = document.FileName,

            BlobUrl = uploadResult.BlobUrl,

            ContentType = document.ContentType,

            FileSize = document.FileSize,

            UploadedOn = document.UploadedOn
        };
    }


    public async Task<Stream> DownloadAsync(
    int documentId,
    CancellationToken cancellationToken)
    {
        var document =
            await _documentRepository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException(
                $"Document '{documentId}' not found.");
        }

        return await _storageService.DownloadFileAsync(
            "Documents",
            document.BlobName,
            cancellationToken);
    }


    public async Task DeleteAsync(
    int documentId,
    CancellationToken cancellationToken)
    {
        var document =
            await _documentRepository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException(
                $"Document '{documentId}' not found.");
        }

        await _storageService.DeleteFileAsync(
            "Documents",
            document.BlobName,
            cancellationToken);

        _documentRepository.Remove(document);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}