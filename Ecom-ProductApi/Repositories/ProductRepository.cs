using Dapper;
using Ecom_ProductApi.Models.DTos;
using System.Data;

namespace Ecom_ProductApi.Repositories;

public class ProductRepository(IDbConnection _db): IProductRepository
{

    public async Task<Guid> InsertProductWithImagesAsync(ProductDto product,
        List<ProductImageDto> images, CancellationToken token = default)
    {
        EnsureOpen(token);

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

    private void EnsureOpen(CancellationToken ct)
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }



}
