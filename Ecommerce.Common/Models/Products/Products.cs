using Microsoft.AspNetCore.Http;

namespace Ecommerce.Common.Models.Products;


public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ProductImageDto
{
    public Guid ImageId { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }  // Provided by Order/User service
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProductForImage
{
    public Guid CategoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string SKU { get; set; } = default!;
    public bool IsActive { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}

public class ProductImage
{
    public string ImageUrl { get; set; } = default!;
    public int SortOrder { get; set; }
}

public class ProductWithImage
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string SKU { get; set; } = default!;

    public string Category { get; set; } = default!;
    public string CategoryDescription { get; set; } = default!;

    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
}


