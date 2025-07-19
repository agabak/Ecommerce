using Confluent.Kafka;
using ECom.Infrastructure.DataAccess.Product.Repositories;
using Ecommerce.Common.Models.Products;
using Ecommerce.Common.Services.Files;
using Ecommerce.Common.Services.Kafka;

namespace ECom.Infrastructure.DataAccess.Product.Services;

public class ProductService(IProductRepository repository,
    IBlobService blobService, IProducerService producerService) : IProductService
{
    private const string Topic_Create_Inventory = "Create.Inventory";

    public async Task<Guid> InsertProductWithImagesAsync(ProductForImage product, CancellationToken token = default)
    {
        var images = new List<ProductImage>();
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
                    images.Add(new ProductImage
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

    public async Task<List<ProductWithImage>> GetDetailedProductsAsync(CancellationToken cancellation = default)
    {
        return await repository.GetDetailedProductsAsync(cancellation);
    }

    public async Task<ProductWithImage?> GetProductByIdAsync(Guid productId, CancellationToken token = default)
    {
        return await repository.GetProductByIdAsync(productId, token);
    }

    private async Task ProcessCreateInventory(Guid productId, CancellationToken token)
    {
        if (productId != Guid.Empty)
        {
            var deliveryResult = await producerService.ProduceAsync(Topic_Create_Inventory, productId.ToString(), productId.ToString(), token);

            if (deliveryResult != null && deliveryResult.Status == PersistenceStatus.Persisted)
            {
                await repository.MarkProductInventorySentAsync(productId, token);
            }
            else
            {
                throw new Exception($"Failed to produce message to topic {Topic_Create_Inventory} for ProductId: {productId}. Status: {deliveryResult?.Status}");
            }
        }
    }
}
