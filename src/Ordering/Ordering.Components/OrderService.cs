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

    public async Task<Order> SubmitOrders(List<OrderItem> orderItems)
    {
        var entity = new Order
        {
            Id = NewId.NextGuid(),
            Date = DateTime.UtcNow,
            Customer = "kiyomarss",
            Items = orderItems
        };

        await _dbContext.Set<Order>().AddAsync(entity);

        await _publishEndpoint.Publish(new OrderSubmitted
        {
            RegistrationId = entity.Id,
            RegistrationDate = entity.Date,
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