using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Components.Domain.Entities;
using Ordering.Components.Domain.RepositoryContracts;
using Ordering.Components.DTO;
using Ordering.Components.StateMachines.Events;
using Product.Grpc;

namespace Ordering.Components;

public class OrderService : IOrderService
{
    readonly OrderingDbContext _dbContext;
    readonly IPublishEndpoint _publishEndpoint;
    private readonly ProductService.ProductServiceClient _productClient;

    public OrderService(OrderingDbContext dbContext, IPublishEndpoint publishEndpoint, ProductService.ProductServiceClient productClient)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _productClient = productClient;
    }

    public async Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos)
    {
        var orderId = NewId.NextGuid();
        
        var entity = new Order
        {
            Id = orderId,
            Date = DateTime.UtcNow,
            State = "kiyomarss",
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
    public async Task SubmitOrders2(Guid orderId)
    {
        var order = await _dbContext.Set<Order>().FirstOrDefaultAsync();

        if (order == null)
            throw new Exception("No orders found in database.");

        await _publishEndpoint.Publish(new PaymentCompleted(order.Id));
        var a = await TryReserveProduct(order.Id, 10);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> TryReserveProduct(Guid productId, int quantity)
    {
        var response = await _productClient.ReserveProductAsync(new ReserveProductRequest
        {
            ProductId = productId.ToString(),
            Quantity = quantity
        });

        return response.Success;
    }
}