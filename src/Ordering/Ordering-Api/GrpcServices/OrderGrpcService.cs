using Grpc.Core;
using OrderGrpc;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using ProductGrpc;

namespace Ordering.Api.GrpcServices;

public class OrderGrpcService : OrderProtoService.OrderProtoServiceBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ProductProtoService.ProductProtoServiceClient _productClient;

    public OrderGrpcService(IOrderRepository orderRepository, ProductProtoService.ProductProtoServiceClient productClient)
    {
        _orderRepository = orderRepository;
        _productClient = productClient;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        // ۱. گرفتن لیست آیدی محصولات از درخواست
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        // ۲. گرفتن اطلاعات محصولات (مثلاً قیمت‌ها) از سرویس کاتالوگ از طریق gRPC
        var productResponse = await _productClient.GetProductsByIdsAsync(new GetProductsRequest
        {
            ProductIds = { productIds }
        });

        var orderId = Guid.NewGuid();
        // ۴. ایجاد شیء سفارش و ذخیره در دیتابیس
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

        await _orderRepository.AddOrder(order);

        // ۵. برگرداندن پاسخ
        return new CreateOrderResponse
        {
            OrderId = order.Id.ToString()
        };
    }
}
