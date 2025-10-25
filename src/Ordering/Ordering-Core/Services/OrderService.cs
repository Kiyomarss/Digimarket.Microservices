using MassTransit;
using Ordering.Core.Domain.Entities;
using Ordering.Core.Domain.RepositoryContracts;
using Ordering.Core.DTO;
using Ordering.Core.ServiceContracts;

namespace Ordering.Core.Services;

public class OrderService : IOrderService
{
    readonly IOrderRepository _orderRepository;

    readonly IPublishEndpoint _publishEndpoint;

    public OrderService(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> CreateOrder(OrderDto dto)
    {
        var orderId = NewId.NextGuid();

        var entity = new Order
        {
            Id = orderId,
            State = "Init",
            Customer = dto.Customer,
            Items = dto.Items.Select(x => new OrderItem
            {
                OrderId = orderId,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        };

        await _orderRepository.AddOrder(entity);

        return entity.Id;
    }
}