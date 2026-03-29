using Microsoft.AspNetCore.Http;

namespace MedReability.Application.Interfaces.Storage;

public interface IMediaStorageService
{
    Task<string?> UploadAsync(string prefix, IFormFile? file, CancellationToken cancellationToken = default);
    Task DeleteFileByUrlAsync(string? fileUrl, CancellationToken cancellationToken = default);
}
