using MassTransit;
using Ordering.Components.Domain.Entities;
using Ordering.Components.Domain.RepositoryContracts;
using Ordering.Components.DTO;
using Ordering.Components.ServiceContracts;
using Product.Grpc;

namespace Ordering.Components.Services;

public class OrderService : IOrderService
{
    readonly IOrderRepository _orderRepository;

    readonly IPublishEndpoint _publishEndpoint;
    private readonly ProductService.ProductServiceClient _productClient;

    public OrderService(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint, ProductService.ProductServiceClient productClient)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
        _productClient = productClient;
    }

    public async Task<Guid> CreateOrder(OrderDto dto)
    {
        var orderId = NewId.NextGuid();

        var entity = new Domain.Entities.Order
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