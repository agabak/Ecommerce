namespace Ecom_Client.Models.DTOs;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}


