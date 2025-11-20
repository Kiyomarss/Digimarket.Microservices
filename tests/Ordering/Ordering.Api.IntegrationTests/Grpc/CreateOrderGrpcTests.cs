// tests/Ordering.Api.IntegrationTests/Grpc/CreateOrderGrpcTests.cs
using FluentAssertions;
using Grpc.Net.Client;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Api.IntegrationTests.Fixtures;
using OrderGrpc;
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
    public async Task CreateOrder_Should_Succeed_And_StoreInOutbox()
    {
        await CleanupDatabase();

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
                new OrderItemDto() { ProductId = TestGuids.Guid1, Quantity = 2 }
            }
        };

        // Act
        var response = await client.CreateOrderAsync(request);

        // Assert
        response.OrderId.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var order = await dbContext.Orders
                                   .Include(o => o.Items)
                                   .FirstOrDefaultAsync(o => o.Id == Guid.Parse(response.OrderId));

        order.Should().NotBeNull();

        var outboxMessages = await dbContext.Set<OutboxMessage>()
                                            .Where(m => m.MessageType.Contains("OrderInitiated"))
                                            .ToListAsync();

        outboxMessages.Should().HaveCount(1);
    }
}