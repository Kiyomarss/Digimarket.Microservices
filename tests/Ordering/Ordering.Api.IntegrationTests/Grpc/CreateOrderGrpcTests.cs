// tests/Ordering.Api.IntegrationTests/Grpc/CreateOrderGrpcTests.cs
using FluentAssertions;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrderGrpc;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;
using Shared.Grpc;

namespace Ordering.Api.IntegrationTests.Grpc;

[Collection("ApiIntegration")]
public class CreateOrderGrpcTests : OrderingAppTestBase
{
    public CreateOrderGrpcTests(OrderingAppFactory fixture) : base(fixture) { }

    [Fact]
    public async Task CreateOrder_ViaGrpc_Should_Create_Order()
    {
        await ResetDatabase();

        var grpcClient = new OrderProtoService.OrderProtoServiceClient(Fixture.CreateGrpcChannel());

        var request = new CreateOrderRequest
        {
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
                                   .FirstAsync(o => o.Id == orderId);

        order.Should().NotBeNull();
        order.Items.Should().HaveCount(2);
    }
}