using Grpc.Core;
using MassTransit;
using Moq;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering_Domain.Domain.Entities;
using Ordering_Domain.Domain.RepositoryContracts;
using ProductGrpc;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.UnitTests.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ProductProtoService.ProductProtoServiceClient> _productClientMock;

        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _productClientMock = new Mock<ProductProtoService.ProductProtoServiceClient>();

            _handler = new CreateOrderCommandHandler(
                                                     _orderRepositoryMock.Object,
                                                     _productClientMock.Object,
                                                     _publishEndpointMock.Object
                                                    );
        }

        private AsyncUnaryCall<GetProductsResponse> BuildGrpcResponse(GetProductsResponse response)
        {
            return new AsyncUnaryCall<GetProductsResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }

        private CreateOrderCommand SampleRequest => new()
        {
            Customer = "Kiyomarth",
            Items =
            {
                new CreateOrderCommand.OrderItemDto
                {
                    ProductId = "00000000-0000-0000-0000-000000000002",
                    Quantity = 2
                },
                new CreateOrderCommand.OrderItemDto
                {
                    ProductId = "00000000-0000-0000-0000-000000000003",
                    Quantity = 3
                }
            }
        };

        private GetProductsResponse SampleGrpcResponse => new()
        {
            Products =
            {
                new ProductInfo
                {
                    ProductId = "00000000-0000-0000-0000-000000000002",
                    ProductName = "Test1",
                    Price = 100
                },
                new ProductInfo
                {
                    ProductId = "00000000-0000-0000-0000-000000000003",
                    ProductName = "Test2",
                    Price = 200
                }
            }
        };

        // ✅ TEST 1 — Happy Path کامل
        [Fact]
        public async Task Handle_Should_Create_Order_And_Publish_Event()
        {
            var request = SampleRequest;
            var grpcResponse = SampleGrpcResponse;

            // Mock GRPC
            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(BuildGrpcResponse(grpcResponse));

            Order capturedOrder = null;

            _orderRepositoryMock
                .Setup(x => x.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(o => capturedOrder = o)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(capturedOrder);

            Assert.Equal("Kiyomarth", capturedOrder.Customer);
            Assert.Equal("Init", capturedOrder.State);
            Assert.Equal(2, capturedOrder.Items.Count);

            var item1 = capturedOrder.Items.First(i => i.ProductName == "Test1");
            Assert.Equal(2, item1.Quantity);
            Assert.Equal(100, item1.Price);

            var item2 = capturedOrder.Items.First(i => i.ProductName == "Test2");
            Assert.Equal(3, item2.Quantity);
            Assert.Equal(200, item2.Price);

            _publishEndpointMock.Verify(x =>
                x.Publish(It.Is<OrderInitiated>(e =>
                        e.Customer == "Kiyomarth" &&
                        e.Id != Guid.Empty),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        // ✅ TEST 2 — وقتی gRPC محصولی برنمی‌گرداند
        [Fact]
        public async Task Handle_Should_Throw_When_Grpc_Returns_Empty_List()
        {
            var request = SampleRequest;

            var emptyResponse = new GetProductsResponse();

            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildGrpcResponse(emptyResponse));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }


        // ✅ TEST 3 — وقتی gRPC Exception می‌دهد
        [Fact]
        public async Task Handle_Should_Throw_When_Grpc_Throws()
        {
            var request = SampleRequest;

            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Internal, "grpc error")));

            await Assert.ThrowsAsync<RpcException>(() =>
                _handler.Handle(request, CancellationToken.None));

            _orderRepositoryMock.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Never);
        }


        // ✅ TEST 4 — وقتی request و response match نیستند → Single() می‌ترکد
        [Fact]
        public async Task Handle_Should_Throw_When_Quantity_Mismatch()
        {
            var request = new CreateOrderCommand
            {
                Customer = "ABC",
                Items =
                {
                    new CreateOrderCommand.OrderItemDto
                    {
                        ProductId = "id-not-exist",
                        Quantity = 5
                    }
                }
            };

            var grpcResponse = SampleGrpcResponse;

            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildGrpcResponse(grpcResponse));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }


        // ✅ TEST 5 — بررسی اینکه AddOrder دقیقاً یک Order با Id صحیح دریافت کند
        [Fact]
        public async Task Handle_Should_Pass_Order_With_Correct_Id()
        {
            var request = SampleRequest;
            var grpcResponse = SampleGrpcResponse;

            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildGrpcResponse(grpcResponse));

            Guid receivedOrderId = Guid.Empty;

            _orderRepositoryMock
                .Setup(x => x.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(o => receivedOrderId = o.Id)
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.Equal(receivedOrderId, result);
        }


        // ✅ TEST 6 — بررسی ساخت event با داده‌های صحیح
        [Fact]
        public async Task Handle_Should_Publish_Event_With_Correct_Data()
        {
            var request = SampleRequest;
            var grpcResponse = SampleGrpcResponse;

            _productClientMock
                .Setup(x => x.GetProductsByIdsAsync(
                    It.IsAny<GetProductsRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(BuildGrpcResponse(grpcResponse));

            OrderInitiated publishedEvent = null;

            _publishEndpointMock
                .Setup(x => x.Publish(
                    It.IsAny<OrderInitiated>(),
                    It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((evt, _) =>
                {
                    publishedEvent = (OrderInitiated)evt;
                })
                .Returns(Task.CompletedTask);

            await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(publishedEvent);
            Assert.Equal("Kiyomarth", publishedEvent.Customer);
            Assert.NotEqual(Guid.Empty, publishedEvent.Id);
        }
    }
}