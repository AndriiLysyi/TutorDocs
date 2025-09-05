using Microsoft.EntityFrameworkCore;
using TutorDocs.Shared.Data;
using TutorDocs.Shared.Data.Entities;
using TutorDocs.Shared.Extensions;
using TutorDocs.Shared.Models.Dto;

namespace TutorDocs.Shared.Repositories;

public class DocumentRepository : BaseRepository, IDocumentRepository
{
    public DocumentRepository(TutorDocsDbContext context) : base(context)
    {
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid documentId)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId);

        return document?.MapToDocumentDto();
    }

    public async Task<DocumentWithMetadataDto?> GetDocumentWithMetadata(Guid documentId, Guid userId)
    {
        var document = await _context.Documents
            .Where(d => d.Id == documentId)
            .Include(d => d.Owners)
            .Include(d => d.SharedWith)
            .FirstOrDefaultAsync();

        if (document == null)
            return null;
        
        var owner = document.Owners.FirstOrDefault(x => x.UserId == userId) ??
                    document.Owners.FirstOrDefault(o =>
                        o.UserId == document.SharedWith.FirstOrDefault(x => x.UserId == userId)?.SharedBy);
        return owner == null 
            ? null 
            : document.MapToDocumentWithMetadataDto(owner, owner.UserId == userId);
    }

    public async Task<IEnumerable<DocumentWithMetadataDto>> GetUserDocuments(Guid userId)
    {
        var documents = await _context.Documents
            .Include(d => d.Owners)
            .Include(d => d.SharedWith)
            .Where(d => d.Owners.Any(o => o.UserId == userId) ||
                       d.SharedWith.Any(s => s.UserId == userId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(document =>
        {
            var owner = document.Owners.FirstOrDefault(x => x.UserId == userId) ??
                        document.Owners.First(o =>
                            o.UserId == document.SharedWith.First(x => x.UserId == userId)?.SharedBy);
            return document.MapToDocumentWithMetadataDto(owner, owner.UserId == userId);
        });
    }

    public async Task<DocumentDto?> GetDocumentByHash(string fileHash)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.FileHash == fileHash);

        return document?.MapToDocumentDto();
    }

    public DocumentDto CreateDocument(DocumentDto documentDto)
    {
        var document = documentDto.MapToDocument();

        _context.Documents.Add(document);
        return document.MapToDocumentDto();
    }

    public async Task<bool> DeleteDocument(Guid documentId, Guid userId)
    {
        var documentOwner = await _context.DocumentOwners
            .FirstOrDefaultAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId == userId);

        if (documentOwner == null)
            return false;

        if (await TryHardRemoveDocument(documentId, userId)) 
            return true;
        
        _context.DocumentOwners.Remove(documentOwner);
        await RevokeSharedAccess(documentId, userId);
        return true;
    }

    public async Task<bool> HasDocumentOwnership(Guid documentId, Guid userId)
    {
        return await _context.DocumentOwners
            .AnyAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId == userId);
    }

    public void AddDocumentOwnership(Guid documentId, Guid userId, DocumentMetadata metadata)
    {
        var documentOwner = new DocumentOwner
        {
            DocumentId = documentId,
            UserId = userId,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentOwners.Add(documentOwner);
    }
    

    private async Task<bool> HasOtherOwnersAsync(Guid documentId, Guid excludeUserId)
    {
        return await _context.DocumentOwners
            .AnyAsync(docOwner => docOwner.DocumentId == documentId && docOwner.UserId != excludeUserId);
    }
    
    private async Task RevokeSharedAccess(Guid documentId, Guid sharedByUserId)
    {
        var itemsToRemove= await _context.AccessControlLists
            .Where(access => access.DocumentId == documentId && access.SharedBy == sharedByUserId)
            .ToListAsync();
        if (itemsToRemove.Count != 0)
        {
            _context.AccessControlLists.RemoveRange(itemsToRemove);
        }
    }
    
    private async Task<bool> TryHardRemoveDocument(Guid documentId, Guid userId)
    {
        var hasOtherOwners = await HasOtherOwnersAsync(documentId, userId);
        if (hasOtherOwners) 
            return false;
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null) 
            return false;
        _context.Documents.Remove(document);
        return true;
    }
}