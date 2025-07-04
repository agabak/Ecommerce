using Ecom_ProductApi.Models.DTos;

namespace Ecom_ProductApi.Services
{
    public interface IProductService
    {
        Task<Guid> InsertProductWithImagesAsync(ProductDto product, CancellationToken token = default);
    }
}
