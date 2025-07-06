namespace Ecom_Client.Models.DTOs;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string Sku { get; set; }
    public string Category { get; set; }
    public string CategoryDescription { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public string ImageUrl { get; set; }
    public int SortOrder { get; set; }
}


