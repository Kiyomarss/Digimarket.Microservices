using FluentAssertions;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;
using Shared.IntegrationEvents.Ordering;

namespace Ordering.Application.IntegrationTests.Orders.Events.Handlers;

public class CreateOrderCommandHandlerTests : OrderingAppTestBase
{
    public CreateOrderCommandHandlerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateOrder_Should_Publish_OrderInitiated_Event()
    {
        await ResetDatabase();

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderCommand.OrderItemDto>
            {
                new() { ProductId = TestGuids.Guid1, Quantity = 2 },
                new() { ProductId = TestGuids.Guid2, Quantity = 1 }
            }
        };

        await Sender.Send(command);

        // Assert
        (await Harness.Published.Any<OrderInitiated>()).Should().BeTrue();
    }
}