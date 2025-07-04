namespace Ecom_ProductApi.Models.DTos;

public class ProductDto
{
    public Guid CategoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string SKU { get; set; } = default!;
    public bool IsActive { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}

public class ProductImageDto
{
    public string ImageUrl { get; set; } = default!;
    public int SortOrder { get; set; }
}

public class ProductWithImageDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string SKU { get; set; } = default!;

    public string Category { get; set; } = default!;
    public string CategoryDescription { get; set; } = default!;

    public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
}


