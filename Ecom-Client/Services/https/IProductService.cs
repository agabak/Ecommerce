using Ecom_Client.Models.DTOs;

namespace Ecom_Client.Services.https
{
    public interface IProductService
    {
        Task<ProductDto> GetProductByIdAsync(Guid productId);
        Task<List<ProductDto>> GetProductsAsync();
    }
}