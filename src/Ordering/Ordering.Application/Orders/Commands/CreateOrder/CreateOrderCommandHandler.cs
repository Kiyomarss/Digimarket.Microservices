using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.Services;
using BuildingBlocks.UnitOfWork;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.ValueObjects;
using Ordering.Application.RepositoryContracts;
using Ordering.Application.Services;
using ProductGrpc;
using OrderItem = ProductGrpc.OrderItem;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUser;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IProductService productService,
        ICurrentUserService currentUser)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _productService = productService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. گرفتن محصولات از gRPC
        var productResponse = await FetchProductsAsync(request.Items, cancellationToken);

        // 2. ایجاد آبجکت Order
        var order = await CreateOrderFromProducts(request, productResponse);
        
        // 4. ذخیره در دیتابیس
        await _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<ReserveProductsResponse> FetchProductsAsync(List<CreateOrderCommand.OrderItemDto> orderItemDtos, CancellationToken ct)
    {
        var items = orderItemDtos.Select(x => new OrderItem()
        {
            ProductId = x.ProductId, Quantity = x.Quantity,
        });
        var request = new ReserveProductsRequest();
        request.Items.AddRange(items);

        var response = await _productService.ReserveProductsAsync(request, ct);

        if (response == null || response.Products.Count == 0)
            throw new ExternalServiceException("Products not found in gRPC service.");

        return response;
    }

    private async Task<Order> CreateOrderFromProducts(
        CreateOrderCommand request,
        ReserveProductsResponse response)
    {
        var userId = await _currentUser.GetRequiredUserId();

        var items = response.Products
                            .Select(product =>
                            {
                                var reqItem = request.Items
                                                     .Single(i => i.ProductId == product.ProductId);

                                return new OrderItemData(
                                                                 Guid.Parse(product.ProductId),
                                                                 product.Price,
                                                                 reqItem.Quantity);
                            })
                            .ToList();

        var order = Order.Create(userId, items);
        
        return order;
    }
}