using Catalog.Components.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Catalog.Components;

public class ProductService : IProductService
{
    readonly ProductDbContext _dbContext;
    readonly IPublishEndpoint _publishEndpoint;

    public ProductService(ProductDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Product> SubmitCatalogs()
    {
        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Name = $"Test Product {Guid.NewGuid().ToString()[..5]}",
            Description = "This is a fake product for testing.",
            Price = 99.99m,
            StockQuantity = 100,
            Images = new List<ProductImage>
            {
                new ProductImage
                {
                    Id = Guid.NewGuid(),
                    Url = "https://via.placeholder.com/150",
                    AltText = "Test Image",
                    ProductId = Guid.Empty // مقداردهی می‌شه بعد از SaveChanges
                }
            },
            CategoryId = new Guid("d56be165-fe7d-460c-ad0a-016d90a31dbc"), // فقط شناسه دسته‌بندی استفاده می‌شود

        };

        await _dbContext.Set<Product>().AddAsync(entity);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateProductException("Duplicate registration", exception);
        }

        return entity;
    }
}