using Ecom_Client.Models.DTOs;

namespace Ecom_Client.Services.https;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("api/products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ProductDto>>()
                ?? [];
    }
    public async Task<ProductDto> GetProductByIdAsync(Guid productId)
    {
        var response = await _httpClient.GetAsync($"api/products/{productId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductDto>() ??
               throw new KeyNotFoundException($"Product with id {productId} not found.");
    }
}

