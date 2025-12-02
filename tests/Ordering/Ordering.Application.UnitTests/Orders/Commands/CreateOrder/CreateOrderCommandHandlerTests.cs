// tests/Ordering.Application.UnitTests/Orders/Commands/CreateOrder/CreateOrderCommandHandlerTests.cs

using BuildingBlocks.Services;
using FluentAssertions;
using Moq;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Services;
using Shared;

namespace Ordering.Application.UnitTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
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

    private static CreateOrderCommandHandler CreateHandler(
        Mock<IOrderRepository>? orderRepo = null,
        Mock<IPublishEndpoint>? publish = null,
        Mock<IUnitOfWork>? uow = null,
        Mock<IProductService>? product = null,
        Mock<ICurrentUserService>? user = null)
    {
        user ??= new Mock<ICurrentUserService>();
        user.Setup(x => x.GetRequiredUserId()).ReturnsAsync(TestGuids.Guid3);

        return new CreateOrderCommandHandler(
            orderRepo?.Object ?? new Mock<IOrderRepository>().Object,
            publish?.Object ?? new Mock<IPublishEndpoint>().Object,
            uow?.Object ?? new Mock<IUnitOfWork>().Object,
            product?.Object ?? new Mock<IProductService>().Object,
            user.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_Should_CreateOrder_PublishEvent_And_Save()
    {
        var orderRepoMock = new Mock<IOrderRepository>();
        var publishMock = new Mock<IPublishEndpoint>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var productServiceMock = new Mock<IProductService>();

        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid1, ProductName = "Product A", Price = 1500 });
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid2, ProductName = "Product B", Price = 2500 });

        productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        var handler = CreateHandler(
            orderRepo: orderRepoMock,
            publish: publishMock,
            uow: unitOfWorkMock,
            product: productServiceMock);

        var command = CreateValidCommand();

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBe(Guid.Empty);

        orderRepoMock.Verify(r => r.AddOrder(It.Is<Order>(o =>
            o.Customer == "Ali Ahmadi" &&
            o.UserId == TestGuids.Guid3 &&
            o.Items.Count == 2 &&
            o.Items.Any(i => i.ProductId.ToString() == TestGuids.Guid1 && i.Quantity == 2) &&
            o.Items.Any(i => i.ProductId.ToString() == TestGuids.Guid2 && i.Quantity == 1)
        )), Times.Once);

        publishMock.Verify(p => p.Publish(
            It.Is<OrderInitiated>(e =>
                e.Customer == "Ali Ahmadi" &&
                e.Id == orderId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyProductResponse_Should_Throw_InvalidOperationException()
    {
        var productServiceMock = new Mock<IProductService>();
        productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetProductsResponse());

        var handler = CreateHandler(product: productServiceMock);
        var command = CreateValidCommand();

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_NullProductResponse_Should_Throw_InvalidOperationException()
    {
        var productServiceMock = new Mock<IProductService>();
        productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetProductsResponse)null!);

        var handler = CreateHandler(product: productServiceMock);
        var command = CreateValidCommand();

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }

    [Fact]
    public async Task Handle_Should_Map_ProductName_Price_And_Quantity_Correctly()
    {
        var productServiceMock = new Mock<IProductService>();
        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo { ProductId = TestGuids.Guid1, ProductName = "Test Product", Price = 999 });

        productServiceMock
            .Setup(x => x.GetProductsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(grpcResponse);

        var orderRepoMock = new Mock<IOrderRepository>();
        Order? capturedOrder = null;
        orderRepoMock.Setup(r => r.AddOrder(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o);

        var handler = CreateHandler(orderRepo: orderRepoMock, product: productServiceMock);

        var command = new CreateOrderCommand
        {
            Customer = "Test",
            Items = { new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Guid1, Quantity = 5 } }
        };

        await handler.Handle(command, CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder.UserId.Should().Be(TestGuids.Guid3);
        var item = capturedOrder.Items.Single();
        item.ProductName.Should().Be("Test Product");
        item.Price.Should().Be(999);
        item.Quantity.Should().Be(5);
    }
}