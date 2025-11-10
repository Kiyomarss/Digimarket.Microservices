using FluentAssertions;
using Grpc.Net.Client;
using Grpc.Core;
using OrderGrpc;

public class OrderGrpcTests : IClassFixture<OrderApiFactory>
{
    private readonly OrderProtoService.OrderProtoServiceClient _client;

    public OrderGrpcTests(OrderApiFactory factory)
    {
        // اطمینان از اینکه HttpClient/GrpcChannel ساخته شده است:
        var httpClient = factory.CreateClient(); // این خط باعث ساخت ConfigureClient می‌شود
        var channel = factory.GrpcChannel;

        // روش ایمن: ساخت CallInvoker و پاس دادن به ctor
        var callInvoker = channel.CreateCallInvoker();
        _client = new OrderProtoService.OrderProtoServiceClient(callInvoker);
    }

    [Fact]
    public async Task CreateOrder_Should_Return_OrderId()
    {
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

        var response = await _client.CreateOrderAsync(request);

        response.OrderId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(response.OrderId, out _).Should().BeTrue();
    }
}