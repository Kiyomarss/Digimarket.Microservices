using BuildingBlocks.CQRS;
using MassTransit;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Core.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ProductProtoService.ProductProtoServiceClient _productClient;

    public CreateOrderCommandHandler(IOrderRepository orderRepository,
                                     ProductProtoService.ProductProtoServiceClient productClient,
                                     IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _productClient = productClient;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var productResponse = await _productClient.GetProductsByIdsAsync(new GetProductsRequest
        {
            ProductIds = { productIds }
        });

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Customer = request.Customer,
            State = "Init",
            Items = productResponse.Products.Select(i => new OrderItem
            {
                OrderId = orderId,
                ProductId = Guid.Parse(i.ProductId),
                ProductName = i.ProductName,
                Price = i.Price
            }).ToList()
        };

        foreach (var item in order.Items)
            item.Quantity = request.Items.Single(x => x.ProductId == item.ProductId.ToString()).Quantity;
        
        var orderInitiated = new OrderInitiated {Id = orderId, Date = DateTime.UtcNow,  Customer = request.Customer};
        await _publishEndpoint.Publish(orderInitiated, cancellationToken);

        await _orderRepository.AddOrder(order);
        return orderId;
    }
}