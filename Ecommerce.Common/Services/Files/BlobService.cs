using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Common.Services.Files;

public class BlobService : IBlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(BlobContainerClient containerClient)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
    }

    private BlobClient GetBlobClient(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentNullException("Blob name cannot be null or empty.", nameof(blobName));
        return _containerClient.GetBlobClient(blobName);
    }

    /// <summary>
    /// Uploads a file to blob storage.
    /// </summary>
    public async Task<string> UploadFileAsync(IFormFile file, string blobName, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentNullException(nameof(file), "File cannot be null or empty.");

        var blobClient = GetBlobClient(blobName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);

        return blobClient.Uri.ToString();
    }

    /// <summary>
    /// Downloads a blob as a stream.
    /// </summary>
    public async Task<Stream> DownloadFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException($"Blob '{blobName}' not found.");

        var downloadInfo = await blobClient.DownloadAsync(cancellationToken).ConfigureAwait(false);

        return downloadInfo.Value.Content;
    }

    /// <summary>
    /// Gets the content type of a blob.
    /// </summary>
    public async Task<string> GetContentTypeAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException($"Blob '{blobName}' not found.");

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return properties.Value.ContentType ?? "application/octet-stream";
    }

    /// <summary>
    /// Deletes a blob if it exists.
    /// </summary>
    public async Task DeleteFileAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException($"Blob '{blobName}' not found.");

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
