// tests/Ordering.Api.IntegrationTests/Grpc/CreateOrderGrpcTests.cs
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using OrderGrpc;
using Ordering.Api.IntegrationTests.Fixtures;
using Shared;
using Xunit;

namespace Ordering.Api.IntegrationTests.Grpc;

[Collection("ApiIntegration")]
public class CreateOrderGrpcTests : IClassFixture<OrderingApiFactory>
{
    private readonly OrderingApiFactory _factory;

    public CreateOrderGrpcTests(OrderingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrder_Should_Create_Order_And_Publish_Event()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // راه‌حل صحیح برای .NET 8+
        var httpClient = _factory.CreateClient();
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        var client = new OrderProtoService.OrderProtoServiceClient(channel);

        var request = new CreateOrderRequest
        {
            Customer = "Ali Ahmadi",
            Items =
            {
                new OrderItemDto { ProductId = TestGuids.Guid1, Quantity = 2 },
                new OrderItemDto { ProductId = TestGuids.Guid2, Quantity = 1 }
            }
        };

        // Act
        var response = await client.CreateOrderAsync(request);

        // Assert 1: پاسخ gRPC
        response.OrderId.Should().NotBeEmpty();
        Guid.Parse(response.OrderId).Should().NotBe(Guid.Empty); // درست شد!

        // Assert 2: Order در دیتابیس ذخیره شده
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var order = await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == Guid.Parse(response.OrderId));

        order.Should().NotBeNull();
        order!.Customer.Should().Be("Ali Ahmadi");
        order.Items.Should().HaveCount(2);

        // Assert 3: پیام در Outbox ذخیره شده
        var outboxMessages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.MessageType.Contains("OrderInitiated"))
            .ToListAsync();

        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].Body.Should().Contain("Ali Ahmadi");
    }
}