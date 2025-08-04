using Dapper;
using Ecommerce.Common.Models.Products;

namespace ECom.Infrastructure.DataAccess.Product.Repositories;

public class ProductRepository :DataAccessProvider, IProductRepository
{
    public ProductRepository(string connection):base(connection)
    {
    }

    public async Task<Guid> InsertProductWithImagesAsync(ProductForImage product,
        List<ProductImage> images, CancellationToken token = default)
    {

        using var dbConnection = GetOpenConnection();
        using var tran = dbConnection.BeginTransaction();
        try
        {
            // 1. Insert product and get ProductId
            const string productSql = @"
            INSERT INTO Products
                (ProductName, Description, Price, CategoryId, SKU, CreatedAt, IsActive)
            OUTPUT INSERTED.ProductId
            VALUES
                (@ProductName, @Description, @Price, @CategoryId, @SKU, SYSDATETIME(), @IsActive);";

            var productId = await dbConnection.ExecuteScalarAsync<Guid>(
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
                await dbConnection.ExecuteAsync(
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

        using var dbConnection = GetOpenConnection();   

        _ = await dbConnection.QueryAsync<ProductWithImage, ProductImage, ProductWithImage>(
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

        var productDict = new Dictionary<Guid, ProductWithImage>();
        using var dbConnection = GetOpenConnection();
        var result = await dbConnection.QueryAsync<ProductWithImage, ProductImage, ProductWithImage>(
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
        using var dbConnection = GetOpenConnection();
        return dbConnection.ExecuteAsync(new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: token));
    }

    public async Task<List<Guid>> GetProductsNotInInventoryAsync(CancellationToken cancellation)
    {
        // This method retrieves all product IDs from the Products table where IsInventory 0.
        const string sql = @"
            SELECT ProductId
            FROM Products
            WHERE IsInventory = 0;";
        using var dbConnection = GetOpenConnection();
        return (await dbConnection.QueryAsync<Guid>(
            new CommandDefinition(sql, cancellationToken: cancellation)
        )).ToList();
    }
}
