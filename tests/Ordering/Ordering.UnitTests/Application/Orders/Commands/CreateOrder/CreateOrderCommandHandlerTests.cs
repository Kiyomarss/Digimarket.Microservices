// tests/Ordering.Application.UnitTests/Orders/Commands/CreateOrder/CreateOrderCommandHandlerTests.cs

using BuildingBlocks.UnitOfWork;
using FluentAssertions;
using Grpc.Core;
using MassTransit;
using Moq;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering.Core.Services;
using Ordering.UnitTests.TestHelpers;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.UnitTests.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();

    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(
            _orderRepoMock.Object,
            _publishMock.Object,
            _unitOfWorkMock.Object,
            _productServiceMock.Object);
    }

    private CreateOrderCommand CreateSampleRequest()
    {
        return new CreateOrderCommand
        {
            Customer = "Kiyomarth",
            Items =
            {
                new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product1, Quantity = 2 },
                new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product2, Quantity = 3 }
            }
        };
    }

    [Fact]
    public async Task Handle_Should_Create_Order_And_Publish_Event()
    {
        // Arrange
        var request = CreateSampleRequest();

        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Product1, ProductName = "A", Price = 100 });
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Product2, ProductName = "B", Price = 200 });

        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        // Act
        var orderId = await _handler.Handle(request, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();

        _orderRepoMock.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
        _publishMock.Verify(x => x.Publish(It.IsAny<OrderInitiated>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Grpc_Returns_Empty()
    {
        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetProductsResponse()); // خالی

        var act = () => _handler.Handle(CreateSampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Grpc_Returns_Null()
    {
        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetProductsResponse)null!); // null برمی‌گرداند

        var act = () => _handler.Handle(CreateSampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_Should_Map_Quantities_Correctly()
    {
        var request = CreateSampleRequest();

        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Product1, ProductName = "P1", Price = 10 });
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Product2, ProductName = "P2", Price = 20 });

        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        Order? captured = null;
        _orderRepoMock.Setup(r => r.AddOrder(It.IsAny<Order>()))
            .Callback<Order>(o => captured = o);

        await _handler.Handle(request, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Items.Should().Satisfy(
            i => i.ProductId == Guid.Parse(TestGuids.Product1) && i.Quantity == 2,
            i => i.ProductId == Guid.Parse(TestGuids.Product2) && i.Quantity == 3
        );
    }

    [Fact]
    public async Task Handle_Should_Map_ProductName_And_Price()
    {
        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Product1, ProductName = "TestA", Price = 800 });

        var request = new CreateOrderCommand
        {
            Customer = "Kiyo",
            Items = { new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product1, Quantity = 1 } }
        };

        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        Order? captured = null;
        _orderRepoMock.Setup(r => r.AddOrder(It.IsAny<Order>()))
            .Callback<Order>(o => captured = o);

        await _handler.Handle(request, CancellationToken.None);

        var item = captured!.Items.Single();
        item.ProductName.Should().Be("TestA");
        item.Price.Should().Be(800);
    }
}