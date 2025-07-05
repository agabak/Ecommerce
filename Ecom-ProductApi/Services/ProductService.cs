using Ecom_ProductApi.Models.DTos;
using Ecom_ProductApi.Repositories;
using Ecommerce.Common.Services.Files;

namespace Ecom_ProductApi.Services;

public class ProductService(IProductRepository repository,
    IBlobService blobService, IKafkaProducerService kafkaProducer) : IProductService
{
    public async Task<Guid> InsertProductWithImagesAsync(ProductDto product, CancellationToken token = default)
    {

        var images = new List<ProductImageDto>();

        if (product.Images == null || product.Images.Count == 0)
        {
            // No images, insert product only
            return await repository.InsertProductWithImagesAsync(product, images, token);
        }

        int sortOrder = 0;

        foreach (var image in product.Images)
        {
            if (image == null)
                continue;

            // Validate content type
            if (string.IsNullOrEmpty(image.ContentType) || !image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid file type. Only images are allowed.");

            if (!string.IsNullOrWhiteSpace(image.FileName))
            {
                var url = await blobService.UploadFileAsync(image, image.FileName, token);
                images.Add(new ProductImageDto
                {
                    ImageUrl = url,
                    SortOrder = sortOrder++
                });
            }
        }

        var productId = await repository.InsertProductWithImagesAsync(product, images, token);

        // Send product creation event to Kafka
        if (productId != Guid.Empty)
        {
            await kafkaProducer.ProduceAsync("Create-Inventory",productId.ToString(), token);

            await repository.UpsertInventoryAsync(productId, token);
        }

        return productId;
    }

    public async Task<List<ProductWithImageDto>> GetDetailedProductsAsync(CancellationToken cancellation = default)
    {
        return await repository.GetDetailedProductsAsync(cancellation);
    }

    public async Task<ProductWithImageDto?> GetProductByIdAsync(Guid productId, CancellationToken token = default)
    {
        return await repository.GetProductByIdAsync(productId, token);
    }
}
