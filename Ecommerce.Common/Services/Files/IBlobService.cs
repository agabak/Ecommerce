using Microsoft.AspNetCore.Http;

namespace Ecommerce.Common.Services.Files;

public interface IBlobService
{
    Task<string> UploadFileAsync(IFormFile file, string blobName, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string blobName, CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(string blobName, CancellationToken cancellationToken = default);

    Task<string> GetContentTypeAsync(string blobName, CancellationToken cancellationToken = default);

}
