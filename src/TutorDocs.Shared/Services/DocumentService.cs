using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Models.Entities;
using TutorDocs.Shared.Models.Enums;
using TutorDocs.Shared.Models.Requests;
using TutorDocs.Shared.Models.Responses;

namespace TutorDocs.Shared.Services;

public class DocumentService : IDocumentService
{
    private readonly TutorDocsDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(TutorDocsDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> CreateDocumentAsync(UploadDocumentRequest request, Guid userId)
    {
        try
        {
            await using var stream = request?.File?.OpenReadStream();
            var fileHash = await ComputeFileHashAsync(stream!);
            
            var existingDocument = await _context.Documents
                .FirstOrDefaultAsync(d => d.FileHash == fileHash);

            if (existingDocument != null)
            {
                var existingOwnership = await _context.DocumentOwners
                    .FirstOrDefaultAsync(docOwner => docOwner.DocumentId == existingDocument.Id && docOwner.UserId == userId);

                if (existingOwnership != null)
                {
                    return new UploadDocumentResponse
                    {
                        DocumentId = existingDocument.Id,
                        Message = "Document already exists in your library",
                        IsSuccess = true,
                        WasExistingFile = true
                    };
                }
                
                var documentOwner = new DocumentOwner
                {
                    DocumentId = existingDocument.Id,
                    UserId = userId,
                    Metadata = new DocumentMetadata
                    {
                        DisplayTitle = request!.DisplayTitle ?? request!.File!.FileName,
                        Description = request.Description,
                        Author = request.Author,
                        Tags = request.Tags.ToList(),
                        Notes = request.Notes
                    }
                };

                _context.DocumentOwners.Add(documentOwner);
                await _context.SaveChangesAsync();

                return new UploadDocumentResponse
                {
                    DocumentId = existingDocument.Id,
                    Message = "Document added to your library",
                    IsSuccess = true,
                    WasExistingFile = true
                };
            }
            
            var document = new Document
            {
                Id = Guid.NewGuid(),
                OriginalFilename = request!.File!.FileName,
                FileHash = fileHash,
                Status = DocumentStatus.Pending
            };

            _context.Documents.Add(document);
            
            var owner = new DocumentOwner
            {
                DocumentId = document.Id,
                UserId = userId,
                Metadata = new DocumentMetadata
                {
                    DisplayTitle = request.DisplayTitle ?? request.File.FileName,
                    Description = request.Description,
                    Author = request.Author,
                    Tags = request.Tags.ToList(),
                    Notes = request.Notes
                }
            };

            _context.DocumentOwners.Add(owner);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} created successfully for user {UserId}", document.Id, userId);

            return new UploadDocumentResponse
            {
                DocumentId = document.Id,
                Message = "Document uploaded successfully",
                IsSuccess = true,
                WasExistingFile = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document for user {UserId}", userId);
            return new UploadDocumentResponse
            {
                DocumentId = Guid.Empty,
                Message = "Error uploading document",
                IsSuccess = false,
                WasExistingFile = false
            };
        }
    }

    public async Task<Document?> GetDocumentAsync(Guid documentId, Guid userId)
    {
        return await _context.Documents
            .Include(d => d.Owners)
            .Include(d => d.SharedWith)
            .Where(d => d.Owners.Any(o => o.UserId == userId) || 
                       d.SharedWith.Any(s => s.UserId == userId))
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<IEnumerable<Document>> GetUserDocumentsAsync(Guid userId)
    {
        return await _context.Documents
            .Include(d => d.Owners)
                .ThenInclude(o => o.User)
            .Where(d => d.Owners.Any(o => o.UserId == userId) ||
                       d.SharedWith.Any(s => s.UserId == userId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var documentOwner = await _context.DocumentOwners
            .FirstOrDefaultAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId == userId);

        if (documentOwner == null)
            return false;

        _context.DocumentOwners.Remove(documentOwner);
        
        var hasOtherOwners = await _context.DocumentOwners
            .AnyAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId != userId);

        if (!hasOtherOwners)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static async Task<string> ComputeFileHashAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}