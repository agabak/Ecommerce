using Ecom_ProductApi.Models.DTos;

namespace Ecom_ProductApi.Repositories
{
    public interface IProductRepository
    {
        Task<Guid> InsertProductWithImagesAsync(ProductDto product, List<ProductImageDto> images, CancellationToken token = default);
    }
}
