using Ecommerce.Common.Models.Products;

namespace ECom.Infrastructure.DataAccess.Product.Services;

public interface IProductService
{
    Task<Guid> InsertProductWithImagesAsync(ProductForImage product, CancellationToken token = default);

    Task<List<ProductWithImage>> GetDetailedProductsAsync(CancellationToken token = default);

    Task<ProductWithImage?> GetProductByIdAsync(Guid productId, CancellationToken token = default);
}

