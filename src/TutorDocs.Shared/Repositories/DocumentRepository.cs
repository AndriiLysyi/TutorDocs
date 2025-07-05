using Microsoft.EntityFrameworkCore;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Data.Entities;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly TutorDocsDbContext _context;

    public DocumentRepository(TutorDocsDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid documentId)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId);

        return document?.MapToDocumentDto();
    }

    public async Task<DocumentWithMetadataDto?> GetDocumentWithMetadataAsync(Guid documentId, Guid userId)
    {
        var result = await _context.Documents
            .Include(d => d.Owners)
            .Include(d => d.SharedWith)
            .Where(d => d.Owners.Any(o => o.UserId == userId) || 
                       d.SharedWith.Any(s => s.UserId == userId))
            .Where(d => d.Id == documentId)
            .Select(d => new { 
                Document = d, 
                Owner = d.Owners.FirstOrDefault(o => o.UserId == userId)
            })
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        return result.Owner != null 
            ? result.Document.MapToDocumentWithMetadataDto(result.Owner)
            : result.Document.MapToDocumentWithMetadataDto();
    }

    public async Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocumentsAsync(Guid userId)
    {
        var results = await _context.Documents
            .Include(d => d.Owners)
            .Include(d => d.SharedWith)
            .Where(d => d.Owners.Any(o => o.UserId == userId) ||
                       d.SharedWith.Any(s => s.UserId == userId))
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new { 
                Document = d, 
                Owner = d.Owners.FirstOrDefault(o => o.UserId == userId)
            })
            .ToListAsync();

        return results.Select(r => r.Owner != null 
            ? r.Document.MapToDocumentWithMetadataDto(r.Owner)
            : r.Document.MapToDocumentWithMetadataDto());
    }

    public async Task<DocumentDto?> GetDocumentByHashAsync(string fileHash)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.FileHash == fileHash);

        return document?.MapToDocumentDto();
    }

    public async Task<DocumentDto> CreateDocumentAsync(DocumentDto documentDto)
    {
        var document = new Document
        {
            Id = documentDto.Id,
            OriginalFilename = documentDto.OriginalFilename,
            FileHash = documentDto.FileHash,
            Status = documentDto.Status,
            CreatedAt = documentDto.CreatedAt,
            UpdatedAt = documentDto.UpdatedAt
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return document.MapToDocumentDto();
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var documentOwner = await _context.DocumentOwners
            .FirstOrDefaultAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId == userId);

        if (documentOwner == null)
            return false;

        _context.DocumentOwners.Remove(documentOwner);

        var hasOtherOwners = await HasOtherOwnersAsync(documentId, userId);

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

    public async Task<bool> HasDocumentOwnershipAsync(Guid documentId, Guid userId)
    {
        return await _context.DocumentOwners
            .AnyAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId == userId);
    }

    public async Task AddDocumentOwnershipAsync(Guid documentId, Guid userId, DocumentMetadata metadata)
    {
        var documentOwner = new DocumentOwner
        {
            DocumentId = documentId,
            UserId = userId,
            Metadata = metadata,
            AddedAt = DateTime.UtcNow
        };

        _context.DocumentOwners.Add(documentOwner);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOtherOwnersAsync(Guid documentId, Guid excludeUserId)
    {
        return await _context.DocumentOwners
            .AnyAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId != excludeUserId);
    }
}