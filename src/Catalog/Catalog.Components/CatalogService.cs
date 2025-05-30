using Catalog.Components.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Catalog.Components;

public class CatalogService : ICatalogService
{
    readonly CatalogDbContext _dbContext;
    readonly IPublishEndpoint _publishEndpoint;

    public CatalogService(CatalogDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Catalog> SubmitCatalogs(List<CatalogItemDto> CatalogItemDtos)
    {
        var CatalogId = NewId.NextGuid();
        
        var entity = new Catalog
        {
            Id = CatalogId,
            Date = DateTime.UtcNow,
            Customer = "kiyomarss",
            Items = CatalogItemDtos.Select(dto => new CatalogItem
            {
                Id = NewId.NextGuid(),
                CatalogId = CatalogId,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Price = dto.Price
            }).ToList()
        };

        await _dbContext.Set<Catalog>().AddAsync(entity);

        await _publishEndpoint.Publish(new CatalogInitiated
        {
            Id = entity.Id,
            Date = entity.Date,
            Customer = entity.Customer,
        });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateCatalogException("Duplicate registration", exception);
        }

        return entity;
    }
}