using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Components.Contracts;

namespace Ordering.Components;

public class OrderService : IOrderService
{
    readonly OrderDbContext _dbContext;
    readonly IPublishEndpoint _publishEndpoint;

    public OrderService(OrderDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos)
    {
        var orderId = NewId.NextGuid();
        
        var entity = new Order
        {
            Id = orderId,
            Date = DateTime.UtcNow,
            Customer = "kiyomarss",
            Items = orderItemDtos.Select(dto => new OrderItem
            {
                Id = NewId.NextGuid(),
                OrderId = orderId,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Price = dto.Price
            }).ToList()
        };

        await _dbContext.Set<Order>().AddAsync(entity);

        await _publishEndpoint.Publish(new OrderInitiated
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
            throw new DuplicateOrderException("Duplicate registration", exception);
        }

        return entity;
    }
}