using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using MedReability.Application.Interfaces.Storage;
using Microsoft.Extensions.Options;

namespace MedReability.Api.Storage;

public class S3MediaStorageService(IOptions<S3StorageOptions> options) : IMediaStorageService
{
    private readonly S3StorageOptions _options = options.Value;

    public async Task<string?> UploadAsync(string prefix, IFormFile? file, CancellationToken cancellationToken = default)
    {
        return await UploadInternalAsync(prefix, file, cancellationToken);
    }

    public async Task DeleteFileByUrlAsync(string? fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        var key = TryExtractKey(fileUrl);
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        using var client = CreateClient();
        await client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key
        }, cancellationToken);
    }

    private async Task<string?> UploadInternalAsync(string prefix, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        using var client = CreateClient();

        var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(client, _options.BucketName);
        if (!bucketExists)
        {
            throw new InvalidOperationException($"S3 bucket '{_options.BucketName}' does not exist.");
        }

        var extension = Path.GetExtension(file.FileName);
        var key = $"{prefix}/{Guid.NewGuid():N}{extension}";

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

    private AmazonS3Client CreateClient()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = _options.ServiceUrl,
            ForcePathStyle = _options.ForcePathStyle
        };

        return new AmazonS3Client(_options.AccessKey, _options.SecretKey, config);
    }

    private string? TryExtractKey(string fileUrl)
    {
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var parsed))
        {
            return null;
        }

        var path = parsed.AbsolutePath.Trim('/');
        var bucketPrefix = $"{_options.BucketName}/";
        if (!path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return path[bucketPrefix.Length..];
    }
}
