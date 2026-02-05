// tests/Ordering.Application.UnitTests/Orders/Commands/CreateOrder/CreateOrderCommandHandlerTests.cs

using BuildingBlocks.Services;
using FluentAssertions;
using Moq;
using Ordering_Domain.Domain.Entities;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.RepositoryContracts;
using Ordering.Application.Services;
using Shared;

namespace Ordering.Application.UnitTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Guid _currentUserId = TestGuids.Guid3;

    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _currentUserServiceMock
            .Setup(x => x.GetRequiredUserId())
            .ReturnsAsync(_currentUserId);

        _handler = new CreateOrderCommandHandler(
            _orderRepoMock.Object,
            _publishMock.Object,
            _unitOfWorkMock.Object,
            _productServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    private CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand
        {
            Customer = "Ali Ahmadi",
            Items =
            {
                new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Guid1, Quantity = 2 },
                new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Guid2, Quantity = 1 }
            }
        };
    }

    [Fact]
    public async Task Handle_ValidRequest_Should_CreateOrder_PublishEvent_And_Save()
    {
        // Arrange
        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid1, ProductName = "Product A", Price = 1500 });
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid2, ProductName = "Product B", Price = 2500 });

        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        var command = CreateValidCommand();

        // Act
        var orderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBe(Guid.Empty);

        _currentUserServiceMock.Verify(r => r.GetRequiredUserId(), Times.Once);
        
        _orderRepoMock.Verify(r => r.Add(It.Is<Order>(o =>
            o.Customer == "Ali Ahmadi" &&
            o.UserId == _currentUserId &&
            o.Items.Count == 2 &&
            o.Items.Any(i => i.ProductId.ToString() == TestGuids.Guid1 && i.Quantity == 2) &&
            o.Items.Any(i => i.ProductId.ToString() == TestGuids.Guid2 && i.Quantity == 1)
        )), Times.Once);

        _publishMock.Verify(p => p.Publish(
            It.Is<OrderInitiated>(e =>
                e.Customer == "Ali Ahmadi" &&
                e.Id == orderId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyProductResponse_Should_Throw_InvalidOperationException()
    {
        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetProductsResponse());

        var command = CreateValidCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_NullProductResponse_Should_Throw_InvalidOperationException()
    {
        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetProductsResponse)null!);

        var command = CreateValidCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_Should_Map_ProductName_Price_And_Quantity_Correctly()
    {
        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid1, ProductName = "Test Product", Price = 999 });

        _productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        var command = new CreateOrderCommand
        {
            Customer = "Test",
            Items = { new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Guid1, Quantity = 5 } }
        };

        Order? capturedOrder = null;
        _orderRepoMock.Setup(r => r.Add(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o);

        await _handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder.UserId.Should().Be(_currentUserId);
        var item = capturedOrder!.Items.Single();
        item.ProductName.Should().Be("Test Product");
        item.Price.Should().Be(999);
        item.Quantity.Should().Be(5);
    }
}