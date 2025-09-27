namespace TutorDocs.Shared.Services;

public interface IS3Service
{
    Task<string> UploadFile(string key, Stream fileStream, string contentType);
    
    Task<string> GeneratePreSignedDownloadUrl(string key, TimeSpan expiration);
    Task<Stream> DownloadFile(string key);
    
    Task<bool> DeleteFile(string key);
    Task<bool> FileExists(string key);
    Task<long> GetFileSize(string key);
}