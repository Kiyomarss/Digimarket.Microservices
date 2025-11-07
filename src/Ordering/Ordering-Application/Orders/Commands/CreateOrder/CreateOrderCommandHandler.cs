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

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
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

        // 1. گرفتن محصولات از gRPC
        var productResponse = await FetchProductsAsync(productIds, cancellationToken);

        // 2. ایجاد آبجکت Order
        var order = CreateOrderFromProducts(request, productResponse);

        // 3. انتشار event
        await PublishOrderInitiatedEvent(order.Id, request.Customer, cancellationToken);

        // 4. ذخیره در دیتابیس
        await _orderRepository.AddOrder(order);

        return order.Id;
    }

    // -------------------------------
    // ⬇ بخش های کوچک و تست‌پذیر
    // -------------------------------

    private async Task<GetProductsResponse> FetchProductsAsync(List<string> productIds, CancellationToken ct)
    {
        var response = await _productClient.GetProductsByIdsAsync(
            new GetProductsRequest { ProductIds = { productIds } },
            cancellationToken: ct);

        if (response == null || response.Products.Count == 0)
            throw new InvalidOperationException("Products not found in gRPC service.");

        return response;
    }

    private Order CreateOrderFromProducts(CreateOrderCommand request, GetProductsResponse productResponse)
    {
        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Customer = request.Customer,
            State = "Init",
            Items = new List<OrderItem>()
        };

        foreach (var grpcProduct in productResponse.Products)
        {
            var productId = Guid.Parse(grpcProduct.ProductId);
            var requestedItem = request.Items.Single(i => i.ProductId == grpcProduct.ProductId);

            order.Items.Add(new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                ProductName = grpcProduct.ProductName,
                Price = grpcProduct.Price,
                Quantity = requestedItem.Quantity
            });
        }

        return order;
    }

    private async Task PublishOrderInitiatedEvent(Guid orderId, string customer, CancellationToken ct)
    {
        var evt = new OrderInitiated
        {
            Id = orderId,
            Customer = customer,
            Date = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(evt, ct);
    }
}