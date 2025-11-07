using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using MassTransit;
using Moq;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering.UnitTests.TestHelpers;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.UnitTests.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IPublishEndpoint> _publishMock = new();
    private readonly Mock<ProductProtoService.ProductProtoServiceClient> _grpcMock = new();

    private readonly CreateOrderCommandHandler _handler;
    private readonly Fixture _fixture = new();

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(
            _orderRepoMock.Object,
            _grpcMock.Object,
            _publishMock.Object
        );

        _fixture.Behaviors.Clear(); // جلوگیری از recursion
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    // -------------------------------------------------
    // ✅ Sample Request via Fixture (سالم، ساده، کنترل‌شده)
    // -------------------------------------------------
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

    private AsyncUnaryCall<GetProductsResponse> BuildResponse(GetProductsResponse resp)
        => GrpcTestHelpers.CreateAsyncUnaryCall(resp);


    // =====================================================
    // ✅ TEST 1 — موفقیت کامل
    // =====================================================
    [Fact]
    public async Task Handle_Should_Create_Order_And_Publish_Event()
    {
        var request = CreateSampleRequest();

        var grpcResponse = new GetProductsResponse
        {
            Products =
            {
                new ProductInfo { ProductId = TestGuids.Product1, ProductName = "A", Price = 100 },
                new ProductInfo { ProductId = TestGuids.Product2, ProductName = "B", Price = 200 }
            }
        };

        _grpcMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(BuildResponse(grpcResponse));

        // Act
        var orderId = await _handler.Handle(request, CancellationToken.None);

        // Assert
        orderId.Should().NotBeEmpty();

        _orderRepoMock.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
        _publishMock.Verify(x =>
            x.Publish(It.IsAny<OrderInitiated>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    // =====================================================
    // ✅ TEST 2 — خالی بودن لیست محصولات
    // =====================================================
    [Fact]
    public async Task Handle_Should_Throw_When_Grpc_Returns_Empty()
    {
        _grpcMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(BuildResponse(new GetProductsResponse()));

        var act = async () => await _handler.Handle(CreateSampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }


    // =====================================================
    // ✅ TEST 3 — گراپ‌سی null بدهد
    // =====================================================
    [Fact]
    public async Task Handle_Should_Throw_When_Grpc_Returns_Null()
    {
        _grpcMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(BuildResponse(null));

        var act = async () => await _handler.Handle(CreateSampleRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }


    // =====================================================
    // ✅ TEST 4 — مپ صحیح تعداد(quantity)
    // =====================================================
    [Fact]
    public async Task Handle_Should_Map_Quantities_Correctly()
    {
        var request = CreateSampleRequest();

        var grpcResponse = new GetProductsResponse
        {
            Products =
            {
                new ProductInfo { ProductId = TestGuids.Product1, ProductName = "P1", Price = 10 },
                new ProductInfo { ProductId = TestGuids.Product2, ProductName = "P2", Price = 20 }
            }
        };

        _grpcMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(BuildResponse(grpcResponse));

        Order? captured = null;

        _orderRepoMock
            .Setup(r => r.AddOrder(It.IsAny<Order>()))
            .Callback<Order>(o => captured = o);

        await _handler.Handle(request, CancellationToken.None);

        captured.Should().NotBeNull();

        captured!.Items.Should().Satisfy(
            i => i.ProductId == Guid.Parse(TestGuids.Product1) && i.Quantity == 2,
            i => i.ProductId == Guid.Parse(TestGuids.Product2) && i.Quantity == 3
        );
    }


    // =====================================================
    // ✅ TEST 5 — ProductName و Price صحیح باشند
    // =====================================================
    [Fact]
    public async Task Handle_Should_Map_ProductName_And_Price()
    {
        var grpcResponse = new GetProductsResponse
        {
            Products =
            {
                new ProductInfo { ProductId = TestGuids.Product1, ProductName = "TestA", Price = 800 }
            }
        };

        var request = new CreateOrderCommand
        {
            Customer = "Kiyo",
            Items =
            {
                new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product1, Quantity = 1 }
            }
        };

        _grpcMock
            .Setup(x => x.GetProductsByIdsAsync(
                It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(BuildResponse(grpcResponse));

        Order? captured = null;

        _orderRepoMock
            .Setup(r => r.AddOrder(It.IsAny<Order>()))
            .Callback<Order>(o => captured = o);

        await _handler.Handle(request, CancellationToken.None);

        var item = captured!.Items.Single();

        item.ProductName.Should().Be("TestA");
        item.Price.Should().Be(800);
    }
}