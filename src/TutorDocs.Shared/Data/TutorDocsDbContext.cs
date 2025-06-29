using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TutorDocs.Shared.Models.Entities;

namespace TutorDocs.Shared.Data;

public class TutorDocsDbContext : DbContext
{
    public TutorDocsDbContext(DbContextOptions<TutorDocsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentOwner> DocumentOwners { get; set; }
    public DbSet<AccessControlList> AccessControlLists { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserEntity(modelBuilder);
        ConfigureDocumentEntity(modelBuilder);
        ConfigureDocumentOwnerEntity(modelBuilder);
        ConfigureAccessControlListEntity(modelBuilder);
        ConfigureDocumentChunkEntity(modelBuilder);
    }

    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureDocumentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileHash).IsUnique();
            entity.Property(e => e.OriginalFilename).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureDocumentOwnerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentOwner>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.UserId });
            
            entity.Property(e => e.Metadata)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<DocumentMetadata>(v, (JsonSerializerOptions?)null) ?? new DocumentMetadata()
                  );
            
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Owners)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.User)
                  .WithMany(u => u.OwnedDocuments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureAccessControlListEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessControlList>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.SharedWith)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SharedDocuments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.DocumentId, e.UserId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureDocumentChunkEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PageInfo)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<PageInfo>(v, (JsonSerializerOptions?)null) ?? new PageInfo()
                  );
            
            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Chunks)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DocumentId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}