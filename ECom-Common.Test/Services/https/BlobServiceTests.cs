using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ecommerce.Common.Services.Files;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace ECom_Common.Test.Services.https;

[TestFixture]
public class BlobServiceTests
{
    private BlobContainerClient _containerClient;
    private BlobClient _blobClient;
    private IFormFile _mockFile;
    private BlobService _service;
    private readonly Uri _fakeBlobUri = new("https://fakeaccount.blob.core.windows.net/container/blob");

    [SetUp]
    public void SetUp()
    {
        _containerClient = Substitute.For<BlobContainerClient>();
        _blobClient = Substitute.For<BlobClient>(new Uri(_fakeBlobUri.ToString()), null);
        _service = new BlobService(_containerClient);

        _mockFile = Substitute.For<IFormFile>();
    }


    [Test]
    public async Task UploadFileAsync_UploadsAndReturnsUri()
    {
        // Arrange
        var blobName = "myblob";
        var ms = new MemoryStream(new byte[] { 1, 2, 3 });
        _mockFile.Length.Returns(ms.Length);
        _mockFile.OpenReadStream().Returns(ms);
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);

        _blobClient.UploadAsync(ms, true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Substitute.For<Response<BlobContentInfo>>()));

        _blobClient.Uri.Returns(_fakeBlobUri);

        // Act
        var result = await _service.UploadFileAsync(_mockFile, blobName);

        // Assert
        Assert.That(result, Is.EqualTo(_fakeBlobUri.ToString()));
        await _blobClient.Received(1).UploadAsync(ms, true, Arg.Any<CancellationToken>());
    }

    [Test]
    public void UploadFileAsync_Throws_IfFileIsNull()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _service.UploadFileAsync(null, "blob");
        });
    }

    [Test]
    public void UploadFileAsync_Throws_IfFileIsEmpty()
    {
        _mockFile.Length.Returns(0);
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _service.UploadFileAsync(_mockFile, "blob");
        });
    }

    [Test]
    public async Task DownloadFileAsync_ReturnsStream_WhenBlobExists()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(true, null)));

        var expectedStream = new MemoryStream(new byte[] { 10, 20, 30 });
        var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: expectedStream);
        var response = Response.FromValue(blobDownloadInfo, null);

        _blobClient.DownloadAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));

        // Act
        var result = await _service.DownloadFileAsync(blobName);

        // Assert
        Assert.That(result, Is.EqualTo(expectedStream));
    }

    [Test]
    public void DownloadFileAsync_Throws_IfBlobNotFound()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(false, null)));

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await _service.DownloadFileAsync(blobName);
        });

        Assert.That(ex.Message, Does.Contain(blobName));
    }

    [Test]
    public async Task GetContentTypeAsync_ReturnsContentType()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(true, null)));

        var properties = BlobsModelFactory.BlobProperties(contentType: "image/png");
        var response = Response.FromValue(properties, null);

        _blobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var contentType = await _service.GetContentTypeAsync(blobName);

        Assert.That(contentType, Is.EqualTo("image/png"));
    }

    [Test]
    public async Task GetContentTypeAsync_ReturnsDefault_WhenContentTypeIsNull()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(true, null)));

        var properties = BlobsModelFactory.BlobProperties(contentType: null);
        var response = Response.FromValue(properties, null);

        _blobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var contentType = await _service.GetContentTypeAsync(blobName);

        Assert.That(contentType, Is.EqualTo("application/octet-stream"));
    }

    [Test]
    public void GetContentTypeAsync_Throws_IfBlobNotFound()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(false, null)));

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await _service.GetContentTypeAsync(blobName);
        });

        Assert.That(ex.Message, Does.Contain(blobName));
    }

    [Test]
    public async Task DeleteFileAsync_Deletes_IfBlobExists()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(true, null)));

        _blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(true, null)));

        await _service.DeleteFileAsync(blobName);

        await _blobClient.Received(1).DeleteIfExistsAsync(
            Arg.Any<DeleteSnapshotsOption>(),
            Arg.Any<BlobRequestConditions>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void DeleteFileAsync_Throws_IfBlobNotFound()
    {
        var blobName = "blob";
        _containerClient.GetBlobClient(blobName).Returns(_blobClient);
        _blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(false, null)));

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await _service.DeleteFileAsync(blobName);
        });

        Assert.That(ex.Message, Does.Contain(blobName));
    }

    [Test]
    public void AllMethods_Throw_OnNullOrEmptyBlobName()
    {
        // These should all throw ArgumentException for invalid blob name  
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.DownloadFileAsync(null));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.DownloadFileAsync(""));

        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.GetContentTypeAsync(null));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.GetContentTypeAsync(""));

        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.DeleteFileAsync(null));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.DeleteFileAsync(""));

        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.UploadFileAsync(_mockFile, null));
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.UploadFileAsync(_mockFile, ""));
    }
}


