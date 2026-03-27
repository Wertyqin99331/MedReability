namespace MedReability.Api.Storage;

public interface IMediaStorageService
{
    Task<string?> UploadExerciseMediaAsync(IFormFile? file, CancellationToken cancellationToken = default);
}
