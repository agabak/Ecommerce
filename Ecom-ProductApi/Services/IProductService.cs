using Ecom_ProductApi.Models.DTos;

namespace Ecom_ProductApi.Services
{
    public interface IProductService
    {
        Task<Guid> InsertProductWithImagesAsync(ProductDto product, CancellationToken token = default);

        Task<List<ProductWithImageDto>> GetDetailedProductsAsync(CancellationToken token = default);

        Task<ProductWithImageDto?> GetProductByIdAsync(Guid productId, CancellationToken token = default);
    }
}
