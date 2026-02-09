using BuildingBlocks.CQRS;
using BuildingBlocks.Services;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering_Domain.Domain.Entities;
using Ordering.Application.RepositoryContracts;
using Ordering.Application.Services;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

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
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        // 1. گرفتن محصولات از gRPC
        var productResponse = await FetchProductsAsync(productIds, cancellationToken);

        // 2. ایجاد آبجکت Order
        var order = await CreateOrderFromProducts(request, productResponse);
        
        // 4. ذخیره در دیتابیس
        await _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    // -------------------------------
    // ⬇ بخش های کوچک و تست‌پذیر
    // -------------------------------

    private async Task<GetProductsResponse> FetchProductsAsync(List<string> productIds, CancellationToken ct)
    {
        var response = await _productService.GetProductsByIdsAsync(productIds, ct);

        if (response == null || response.Products.Count == 0)
            throw new InvalidOperationException("Products not found in gRPC service.");

        return response;
    }

    private async Task<Order> CreateOrderFromProducts(
        CreateOrderCommand request,
        GetProductsResponse products)
    {
        var userId = await _currentUser.GetRequiredUserId();

        var order = Order.Create(userId);

        foreach (var product in products.Products)
        {
            var reqItem = request.Items.Single(i => i.ProductId == product.ProductId);

            order.AddItem(
                          Guid.Parse(product.ProductId),
                          product.ProductName,
                          product.Price,
                          reqItem.Quantity);
        }

        return order;
    }
}