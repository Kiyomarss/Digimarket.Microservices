// tests/Ordering.Api.IntegrationTests/Grpc/CreateOrderGrpcTests.cs
using FluentAssertions;
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
public class CreateOrderGrpcTests : OrderApiTestBase
{
    [Fact]
    public async Task CreateOrder_ViaGrpc_Should_CreateOrder_And_StoreInOutbox()
    {
        await CleanupDatabase();

        var grpcClient = new OrderProtoService.OrderProtoServiceClient(
                                                                       Fixture.CreateGrpcChannel());

        var request = new CreateOrderRequest
        {
            Customer = "Ali Ahmadi",
            Items =
            {
                new OrderItemDto { ProductId = TestGuids.Guid1, Quantity = 2 },
                new OrderItemDto { ProductId = TestGuids.Guid2, Quantity = 1 }
            }
        };

        var response = await grpcClient.CreateOrderAsync(request);

        response.OrderId.Should().NotBeEmpty();
        var orderId = Guid.Parse(response.OrderId);

        var order = await DbContext.Orders
                                   .Include(o => o.Items)
                                   .FirstOrDefaultAsync(o => o.Id == orderId);

        order.Should().NotBeNull();
        order!.Customer.Should().Be("Ali Ahmadi");
        order.Items.Should().HaveCount(2);

        var outboxMessages = await DbContext.Set<OutboxMessage>()
                                            .Where(m => m.MessageType.Contains("OrderInitiated"))
                                            .ToListAsync();

        outboxMessages.Should().HaveCount(1);
    }
}