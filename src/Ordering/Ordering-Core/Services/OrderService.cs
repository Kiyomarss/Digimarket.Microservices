using Catalog.Components.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Components.Domain.Entities;
using Ordering.Components.DTO;
using Ordering.Components.ServiceContracts;
using Ordering.Components.StateMachines.Events;
using Product.Grpc;

namespace Ordering.Components;

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

    public async Task<Order> SubmitOrders(List<OrderItemDto> orderItemDtos)
    {
        var orderId = NewId.NextGuid();
        
        var entity = new Order
        {
            Id = orderId,
            State = "kiyomarss",
            Customer = "kiyomarss",
            Items =
            [
                new OrderItem
                {
                    Id = NewId.NextGuid(),
                    OrderId = orderId,
                    ProductId = NewId.NextGuid(),
                    ProductName = "dto.ProductName",
                    Quantity = 8,
                    Price = 10
                }
            ]
        };

        await _orderRepository.AddOrder(entity);



        return entity;
    }

}