// tests/Ordering.Application.IntegrationTests/Orders/Commands/CreateOrder/CreateOrderCommandHandler_IntegrationTests.cs
using FluentAssertions;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Core.Orders.Commands.CreateOrder;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.Application.IntegrationTests.Fixtures;
using Shared;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Application.IntegrationTests.Orders.Commands.CreateOrder;

[Collection("Integration")]
public class CreateOrderCommandHandler_IntegrationTests : IClassFixture<OrderingIntegrationFixture>
{
    private readonly OrderingIntegrationFixture _fixture;
    private readonly ISender _sender;
    private readonly OrderingDbContext _dbContext;
    private readonly ITestHarness _testHarness;

    public CreateOrderCommandHandler_IntegrationTests(OrderingIntegrationFixture fixture)
    {
        _fixture = fixture;
        _sender = fixture.Services.GetRequiredService<ISender>();
        _dbContext = fixture.Services.GetRequiredService<OrderingDbContext>();
        _testHarness = fixture.Services.GetRequiredService<ITestHarness>();
    }

    [Fact]
    public async Task Handle_Should_PersistOrder_And_Save_OutboxMessage()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new CreateOrderCommand
        {
            Customer = "Ali Ahmadi",
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = TestGuids.Guid1, Quantity = 2 },
                new() { ProductId = TestGuids.Guid2, Quantity = 1 }
            }
        };

        // Act
        var orderId = await _sender.Send(command);

        // Assert 1: Order در دیتابیس ذخیره شده
        var order = await _dbContext.Orders
                                    .Include(o => o.Items)
                                    .FirstOrDefaultAsync(o => o.Id == orderId);

        order.Should().NotBeNull();
        order!.Customer.Should().Be("Ali Ahmadi");
        order.Items.Should().HaveCount(2);

        // Assert 2: پیام در Outbox ذخیره شده
        var outboxMessages = await _dbContext.Set<OutboxMessage>()
                                             .Where(m => m.MessageType.Contains("OrderInitiated"))  // اصلاح شده
                                             .ToListAsync();

        outboxMessages.Should().NotBeEmpty("OrderInitiated should be stored in Outbox");
        outboxMessages.Should().HaveCount(1, "Only one event should be published");
        outboxMessages[0].Body.Should().Contain("Ali Ahmadi");
        outboxMessages[0].Body.Should().Contain(orderId.ToString());

        // Assert 3: پیام منتشر شده (از طریق TestHarness)
        var published = await _testHarness.Published.Any<OrderInitiated>(x => 
                                                                             x.Context.Message.Id == orderId && x.Context.Message.Customer == "Ali Ahmadi");

        published.Should().BeTrue();
    }
}