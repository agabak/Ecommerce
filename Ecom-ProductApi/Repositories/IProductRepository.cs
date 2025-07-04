using Ecom_ProductApi.Models.DTos;

namespace Ecom_ProductApi.Repositories
{
    public interface IProductRepository
    {
        Task<Guid> InsertProductWithImagesAsync(ProductDto product, List<ProductImageDto> images, CancellationToken token = default);

        Task<List<ProductWithImageDto>> GetDetailedProductsAsync(CancellationToken token = default);

        Task<ProductWithImageDto?> GetProductByIdAsync(Guid productId, CancellationToken token = default);
    }
}
