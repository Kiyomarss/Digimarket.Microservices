using FluentAssertions;
using Moq;
using Shared.IntegrationEvents.Ordering;
using BuildingBlocks.UnitOfWork;
using MassTransit;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering.Core.Orders.Commands.CreateOrder;
using ProductGrpc;
using Grpc.Core;
using Shared;
using Shared.Grpc; // این using حتماً اضافه شود!

namespace Ordering.Application.UnitTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ProductProtoService.ProductProtoServiceClient> _productClientMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _productClientMock = new Mock<ProductProtoService.ProductProtoServiceClient>(MockBehavior.Loose);
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    private AsyncUnaryCall<GetProductsResponse> BuildResponse(GetProductsResponse resp)
        => GrpcTestHelpers.CreateAsyncUnaryCall(resp);

    [Fact]
    public async Task Handle_ValidRequest_Should_CreateOrder_PublishEvent_And_Save()
    {
        // Arrange
        var grpcResponse = new GetProductsResponse();
        grpcResponse.Products.Add(new ProductInfo
        {
            ProductId = TestGuids.Guid1,
            ProductName = "Test Product",
            Price = 1500
        });

        // استفاده از BuildResponse + GrpcTestHelpers
        _productClientMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(BuildResponse(grpcResponse));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _productClientMock.Object,
            _publishEndpointMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOrderCommand
        {
            Customer = "Ali Ahmadi",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = TestGuids.Guid1, Quantity = 3 }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();

        _orderRepositoryMock.Verify(r => r.AddOrder(It.Is<Order>(o =>
            o.Customer == "Ali Ahmadi" &&
            o.Items.Count == 1 &&
            o.Items[0].ProductName == "Test Product" &&
            o.Items[0].Price == 1500 &&
            o.Items[0].Quantity == 3
        )), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<OrderInitiated>(e =>
                e.Customer == "Ali Ahmadi" &&
                e.Id == orderId
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_Should_Throw_InvalidOperationException()
    {
        // Arrange
        _productClientMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(BuildResponse(new GetProductsResponse()));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _productClientMock.Object,
            _publishEndpointMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateOrderCommand
        {
            Customer = "Test",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = TestGuids.Guid1, Quantity = 1 }
            }
        };

        // Act & Assert
        await FluentActions
            .Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Products not found in gRPC service.");
    }
}