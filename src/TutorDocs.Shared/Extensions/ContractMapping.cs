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

    public static DocumentWithMetadataDto MapToDocumentWithMetadataDto(this Document entity, DocumentOwner owner)
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
            IsOwner = true
        };
    }

    public static DocumentWithMetadataDto MapToDocumentWithMetadataDto(this Document entity)
    {
        return new DocumentWithMetadataDto
        {
            Id = entity.Id,
            OriginalFilename = entity.OriginalFilename,
            FileHash = entity.FileHash,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DisplayTitle = entity.OriginalFilename,
            Description = null,
            Author = null,
            Tags = [],
            Notes = null,
            IsOwner = false
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
            OriginalFilename = request.File?.FileName ?? string.Empty,
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
                DisplayTitle = request.DisplayTitle ?? request.File?.FileName,
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
}