using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;

namespace MedReability.Api.Storage;

public class S3MediaStorageService(IOptions<S3StorageOptions> options) : IMediaStorageService
{
    private readonly S3StorageOptions _options = options.Value;

    public async Task<string?> UploadExerciseMediaAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = _options.ServiceUrl,
            ForcePathStyle = _options.ForcePathStyle
        };

        using var client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);

        var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(client, _options.BucketName);
        if (!bucketExists)
        {
            throw new InvalidOperationException($"S3 bucket '{_options.BucketName}' does not exist.");
        }

        var extension = Path.GetExtension(file.FileName);
        var key = $"exercises/{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();
        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await client.PutObjectAsync(putRequest, cancellationToken);

        var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{_options.BucketName}/{key}";
    }
}
