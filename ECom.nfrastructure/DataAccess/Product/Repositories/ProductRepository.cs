using Dapper;
using Ecommerce.Common.Models.Products;
using System.Data;

namespace ECom.Infrastructure.DataAccess.Product.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnection _db;
    private readonly IDataAccessProvider _provider;
    public ProductRepository(IDataAccessProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _db = _provider.CreateDbConnection();
    }

    public async Task<Guid> InsertProductWithImagesAsync(ProductForImage product,
        List<ProductImage> images, CancellationToken token = default)
    {
        _provider.EnsureConnection(_db);
        using var tran = _db.BeginTransaction();
        try
        {
            // 1. Insert product and get ProductId
            const string productSql = @"
            INSERT INTO Products
                (ProductName, Description, Price, CategoryId, SKU, CreatedAt, IsActive)
            OUTPUT INSERTED.ProductId
            VALUES
                (@ProductName, @Description, @Price, @CategoryId, @SKU, SYSDATETIME(), @IsActive);";

            var productId = await _db.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    productSql,
                    product,
                    tran,
                    cancellationToken: token
                )
            );

            // 2. Insert images
            const string imageSql = @"
            INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder)
            VALUES (@ProductId, @ImageUrl, @SortOrder);";

            foreach (var img in images)
            {
                await _db.ExecuteAsync(
                    new CommandDefinition(
                        imageSql,
                        new { ProductId = productId, img.ImageUrl, img.SortOrder },
                        tran,
                        cancellationToken: token
                    )
                );
            }

            tran.Commit();
            return productId;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<List<ProductWithImage>> GetDetailedProductsAsync(CancellationToken token = default)
    {
        const string sql = @"
                SELECT
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Description,
                    p.SKU,
                    c.CategoryName AS Category,
                    c.Description AS CategoryDescription,
                    pi.ImageUrl,
                    pi.SortOrder
                FROM
                    dbo.Products AS p
                    INNER JOIN dbo.Categories AS c ON c.CategoryId = p.CategoryId
                    LEFT JOIN dbo.ProductImages AS pi ON pi.ProductId = p.ProductId
                ORDER BY
                    p.ProductName,
                    pi.SortOrder;";

        var productDict = new Dictionary<Guid, ProductWithImage>();

        _provider.EnsureConnection(_db);

        _ = await _db.QueryAsync<ProductWithImage, ProductImage, ProductWithImage>(
            new CommandDefinition(sql, cancellationToken: token),
            map: (product, image) =>
            {
                if (!productDict.TryGetValue(product.ProductId, out var existingProduct))
                {
                    existingProduct = product;
                    productDict[product.ProductId] = existingProduct;
                }

                if (image != null && !string.IsNullOrEmpty(image.ImageUrl))
                {
                    existingProduct.Images.Add(image);
                }

                return existingProduct;
            },
            splitOn: "ImageUrl"
        );

        return productDict.Values.ToList();
    }

    public async Task<ProductWithImage?> GetProductByIdAsync(Guid productId, CancellationToken token = default)
    {
        const string sql = @"
                SELECT
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Description,
                    p.SKU,
                    c.CategoryName AS Category,
                    c.Description AS CategoryDescription,
                    pi.ImageUrl,
                    pi.SortOrder
                FROM
                    dbo.Products AS p
                    INNER JOIN dbo.Categories AS c ON c.CategoryId = p.CategoryId
                    LEFT JOIN dbo.ProductImages AS pi ON pi.ProductId = p.ProductId
                WHERE
                    p.ProductId = @ProductId;";

        _provider.EnsureConnection(_db);

        var productDict = new Dictionary<Guid, ProductWithImage>();
        var result = await _db.QueryAsync<ProductWithImage, ProductImage, ProductWithImage>(
            new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: token),
            map: (product, image) =>
            {
                if (!productDict.TryGetValue(product.ProductId, out var existingProduct))
                {
                    existingProduct = product;
                    productDict[product.ProductId] = existingProduct;
                }
                if (image != null && !string.IsNullOrEmpty(image.ImageUrl))
                {
                    existingProduct.Images.Add(image);
                }
                return existingProduct;
            },
            splitOn: "ImageUrl"
        );

        return productDict.Values.FirstOrDefault();
    }

    public Task MarkProductInventorySentAsync(Guid productId, CancellationToken token = default)
    {
        const string sql = @"
            UPDATE Products
            SET IsInventory = 1
            WHERE ProductId = @ProductId;";
    
        _provider.EnsureConnection(_db);
        return _db.ExecuteAsync(new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: token));
    }

    public async Task<List<Guid>> GetProductsNotInInventoryAsync(CancellationToken cancellation)
    {
        // This method retrieves all product IDs from the Products table where IsInventory 0.
        const string sql = @"
            SELECT ProductId
            FROM Products
            WHERE IsInventory = 0;";

        _provider.EnsureConnection(_db);
        return (await _db.QueryAsync<Guid>(
            new CommandDefinition(sql, cancellationToken: cancellation)
        )).ToList();
    }
}
