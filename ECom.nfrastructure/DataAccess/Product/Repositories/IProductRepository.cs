using Ecommerce.Common.Models.Products;

namespace ECom.Infrastructure.DataAccess.Product.Repositories;

public interface IProductRepository
{
    Task<Guid> InsertProductWithImagesAsync(ProductForImage product, List<ProductImage> images, CancellationToken token = default);

    Task<List<ProductWithImage>> GetDetailedProductsAsync(CancellationToken token = default);

    Task<ProductWithImage?> GetProductByIdAsync(Guid productId, CancellationToken token = default);

    Task MarkProductInventorySentAsync(Guid productId, CancellationToken token = default);

    Task<List<Guid>> GetProductsNotInInventoryAsync(CancellationToken cancellation);
}

