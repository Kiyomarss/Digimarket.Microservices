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
        await _publishEndpoint.Publish(new OrderInitiated { Id = Guid.Parse("9336d6b2-68cf-48f8-81c0-5eb335f4571e"), Date = DateTime.UtcNow, Customer = "asd"});
        await _orderRepository.Update();

        return NewId.NextGuid();
    }
}