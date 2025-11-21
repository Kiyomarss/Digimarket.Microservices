// tests/Ordering.Application.IntegrationTests/Orders/Commands/CreateOrder/CreateOrderCommandHandler_IntegrationTests.cs
using FluentAssertions;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Ordering.Core.Orders.Commands.CreateOrder;
using Shared;

namespace Ordering.Application.IntegrationTests.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler_IntegrationTests : OrderingAppTestBase
{
    [Fact]
    public async Task Handle_Should_PersistOrder_And_Save_OutboxMessage()
    {
        // Arrange
        await CleanupDatabase();

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
        var orderId = await Sender.Send(command);

        // Assert 1: Order در دیتابیس ذخیره شده
        var order = await DbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        order.Should().NotBeNull();
        order!.Customer.Should().Be("Ali Ahmadi");
        order.Items.Should().HaveCount(2);

        // Assert 2: پیام در Outbox ذخیره شده
        var outboxMessages = await DbContext.Set<OutboxMessage>()
            .Where(m => m.MessageType.Contains("OrderInitiated"))
            .ToListAsync();
        
        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].Body.Should().Contain(orderId.ToString());
    }
}