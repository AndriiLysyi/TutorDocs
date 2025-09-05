using TutorDocs.Shared.Data.Entities;
using TutorDocs.Shared.Models.Dto;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;
using TutorDocs.Shared.Models.Enums;

namespace TutorDocs.Shared.Extensions;

public static class ContractMapping
{
    public static DocumentDto MapToDocumentDto(this Document entity)
    {
        return new DocumentDto
        {
            Id = entity.Id,
            OriginalFilename = entity.OriginalFilename,
            FileHash = entity.FileHash,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public static DocumentDto MapToDocumentDto(this UploadDocumentRequest entity, string fileHash)
    {
        return new DocumentDto
        {
            Id = Guid.NewGuid(),
            OriginalFilename = entity.File.FileName,
            FileHash = fileHash,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static DocumentWithMetadataDto MapToDocumentWithMetadataDto(this Document entity, DocumentOwner owner, bool isOwner)
    {
        return new DocumentWithMetadataDto
        {
            Id = entity.Id,
            OriginalFilename = entity.OriginalFilename,
            FileHash = entity.FileHash,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DisplayTitle = owner.Metadata.DisplayTitle ?? entity.OriginalFilename,
            Description = owner.Metadata.Description,
            Author = owner.Metadata.Author,
            Tags = owner.Metadata.Tags,
            Notes = owner.Metadata.Notes,
            IsOwner = isOwner,
        };
    }

    public static DocumentResponse MapToDocumentResponse(this DocumentDto dto)
    {
        return new DocumentResponse
        {
            Id = dto.Id,
            OriginalFilename = dto.OriginalFilename,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    public static DocumentResponse MapToDocumentResponse(this DocumentWithMetadataDto dto)
    {
        return new DocumentResponse
        {
            Id = dto.Id,
            OriginalFilename = dto.OriginalFilename,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    public static GetDocumentResponse MapToGetDocumentResponse(this DocumentWithMetadataDto dto)
    {
        return new GetDocumentResponse
        {
            Id = dto.Id,
            OriginalFilename = dto.OriginalFilename,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            DisplayTitle = dto.DisplayTitle,
            Description = dto.Description,
            Author = dto.Author,
            Tags = dto.Tags,
            Notes = dto.Notes
        };
    }

    public static GetUserDocumentsResponse MapToGetUserDocumentsResponse(this IEnumerable<DocumentWithMetadataDto> dtos)
    {
        return new GetUserDocumentsResponse
        {
            Documents = dtos.Select(dto => dto.MapToDocumentResponse())
        };
    }

    public static Document MapToDocument(this UploadDocumentRequest request)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            OriginalFilename = request.File.FileName,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static DocumentOwner MapToDocumentOwner(this UploadDocumentRequest request, Guid documentId, Guid userId)
    {
        return new DocumentOwner
        {
            DocumentId = documentId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Metadata = new DocumentMetadata
            {
                DisplayTitle = request.DisplayTitle ?? request.File.FileName,
                Description = request.Description,
                Author = request.Author,
                Tags = request.Tags.ToList(),
                Notes = request.Notes
            }
        };
    }

    public static UserDto MapToUserDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Email = entity.Email,
            CreatedAt = entity.CreatedAt
        };
    }

    public static DeleteDocumentResponse MapToDeleteResponse(bool isSuccess, string message)
    {
        return new DeleteDocumentResponse
        {
            IsSuccess = isSuccess,
            Message = message
        };
    }

    public static ErrorResponse MapToErrorResponse(string message, int statusCode, string? details = null)
    {
        return new ErrorResponse
        {
            Message = message,
            StatusCode = statusCode,
            Details = details
        };
    }
    
    public static Document MapToDocument(this DocumentDto documentDto)
    {
        return new Document
        {
            Id = documentDto.Id,
            OriginalFilename = documentDto.OriginalFilename,
            FileHash = documentDto.FileHash,
            Status = documentDto.Status,
            CreatedAt = documentDto.CreatedAt,
            UpdatedAt = documentDto.UpdatedAt
        };
    }
    
    public static UploadDocumentResponse MapToUploadDocumentResponse(Guid documentId, string message, bool wasExistingFile,
        bool isSuccess = true)
    {
        return new UploadDocumentResponse
        {
            DocumentId = documentId,
            Message = message,
            IsSuccess = isSuccess,
            WasExistingFile = wasExistingFile
        };
    }
}