using Ecom_ProductApi.Models.DTos;
using Ecom_ProductApi.Repositories;
using Ecommerce.Common.Services.Files;

namespace Ecom_ProductApi.Services;

public class ProductService(IProductRepository repository,
    IBlobService blobService, IKafkaProducerService kafkaProducer) : IProductService
{
    private const string Topic_Create_Inventory = "Create.Inventory";

    public async Task<Guid> InsertProductWithImagesAsync(ProductDto product, CancellationToken token = default)
    {
        var images = new List<ProductImageDto>();
        var unsentProductIds = await repository.GetProductsNotInInventoryAsync(token) ?? new List<Guid>();
        Guid productId;

        // Handle images (if any)
        if (product.Images != null && product.Images.Count > 0)
        {
            int sortOrder = 0;
            foreach (var image in product.Images)
            {
                if (image == null)
                    continue;

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
        }

        productId = await repository.InsertProductWithImagesAsync(product, images, token);
        
        unsentProductIds.Add(productId);

        foreach (var id in unsentProductIds)
        {
            await ProcessCreateInventory(id, token);
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

    private async Task ProcessCreateInventory(Guid productId, CancellationToken token)
    {
        if (productId != Guid.Empty)
        {
            await kafkaProducer.ProduceAsync(Topic_Create_Inventory, productId.ToString(), token);

            await repository.MarkProductInventorySentAsync(productId, token);
        }
    }
}
