using FluentAssertions;
using OrderGrpc;
using Ordering.ApiTests.Grpc.TestServer;

public class OrderGrpcTests : IClassFixture<OrderingApiFactory>
{
    private readonly OrderProtoService.OrderProtoServiceClient _client;

    public OrderGrpcTests(OrderingApiFactory factory)
    {
        // ایجاد HttpClient برای فعال شدن ConfigureClient
        var httpClient = factory.CreateClient();

        // استفاده از GrpcChannel که در Factory ساخته شده
        var channel = factory.GrpcChannel;

        _client = new OrderProtoService.OrderProtoServiceClient(channel);
    }

    [Fact]
    public async Task CreateOrder_Should_Return_OrderId()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Customer = "kiomars",
            Items =
            {
                new OrderItemDto
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 2
                }
            }
        };

        // Act
        var response = await _client.CreateOrderAsync(request);

        // Assert
        response.OrderId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(response.OrderId, out _).Should().BeTrue();
    }
}