using Grpc.Core;
using MassTransit;
using Moq;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering.UnitTests.TestHelpers;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.UnitTests.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IPublishEndpoint> _publishMock;
        private readonly Mock<ProductProtoService.ProductProtoServiceClient> _grpcMock;

        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _publishMock = new Mock<IPublishEndpoint>();
            _grpcMock = new Mock<ProductProtoService.ProductProtoServiceClient>();

            _handler = new CreateOrderCommandHandler(
                _orderRepoMock.Object,
                _grpcMock.Object,
                _publishMock.Object
            );
        }

        // -----------------------------------------------------
        // ✅ Helper Methods
        // -----------------------------------------------------

        private CreateOrderCommand SampleRequest =>
            new CreateOrderCommand
            {
                Customer = "Kiyomarth",
                Items =
                {
                    new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product1, Quantity = 2 },
                    new CreateOrderCommand.OrderItemDto { ProductId = TestGuids.Product2, Quantity = 3 }
                }
            };

        private AsyncUnaryCall<GetProductsResponse> BuildResponse(GetProductsResponse resp)
        {
            return GrpcTestHelpers.CreateAsyncUnaryCall(resp);
        }


        // =====================================================
        // ✅ TEST 1 — موفقیت کامل
        // =====================================================
        [Fact]
        public async Task Handle_Should_Create_Order_And_Publish_Event()
        {
            var request = SampleRequest;

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
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildResponse(grpcResponse));

            // Act
            var orderId = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, orderId);

            _orderRepoMock.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
            _publishMock.Verify(x =>
                x.Publish(It.IsAny<OrderInitiated>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }


        // =====================================================
        // ✅ TEST 2 — گراپ‌سی خروجی خالی بدهد → Exception
        // =====================================================
        [Fact]
        public async Task Handle_Should_Throw_When_Grpc_Returns_Empty()
        {
            var emptyResponse = new GetProductsResponse();

            _grpcMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildResponse(emptyResponse));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(SampleRequest, CancellationToken.None));
        }


        // =====================================================
        // ✅ TEST 3 — گراپ‌سی null بدهد → Exception
        // =====================================================
        [Fact]
        public async Task Handle_Should_Throw_When_Grpc_Returns_Null()
        {
            _grpcMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildResponse(null));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(SampleRequest, CancellationToken.None));
        }


        // =====================================================
        // ✅ TEST 4 — بررسی اینکه Quantity درست مپ شده
        // =====================================================
        [Fact]
        public async Task Handle_Should_Map_Quantities_Correctly()
        {
            var request = SampleRequest;

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
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildResponse(grpcResponse));

            Order? capturedOrder = null;

            _orderRepoMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o)
                .Returns(Task.CompletedTask);

            await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(capturedOrder);

            Assert.Equal(2, capturedOrder!.Items.Single(i => i.ProductId == Guid.Parse(TestGuids.Product1)).Quantity);
            Assert.Equal(3, capturedOrder!.Items.Single(i => i.ProductId == Guid.Parse(TestGuids.Product2)).Quantity);
        }


        // =====================================================
        // ✅ TEST 5 — بررسی مقدار ProductName و Price
        // =====================================================
        [Fact]
        public async Task Handle_Should_Map_ProductName_And_Price()
        {
            var grpcResponse = new GetProductsResponse
            {
                Products =
                {
                    new ProductInfo { ProductId = TestGuids.Product1, ProductName = "TestA", Price = 800 },
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
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildResponse(grpcResponse));

            Order? captured = null;

            _orderRepoMock
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(o => captured = o)
                .Returns(Task.CompletedTask);

            await _handler.Handle(request, CancellationToken.None);

            var item = captured!.Items.Single();

            Assert.Equal("TestA", item.ProductName);
            Assert.Equal(800, item.Price);
        }
    }
}