using BuildingBlocks.Exceptions.Application;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Commands.PayOrder;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;
using Shared;

namespace Ordering.Application.IntegrationTests.Orders.Commands.PayOrder;

public class PayOrderCommandHandlerTests : OrderingAppTestBase
{
    public PayOrderCommandHandlerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Handle_Should_Change_OrderState_To_Paid()
    {
        // Arrange
        await ResetDatabase();

        var order = new OrderBuilder()
                    .WithItems((1, 100))
                    .Build();

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        var command = new PayOrderCommand { Id = order.Id };

        // Act
        await Sender.Send(command);

        // Refresh the entity to get updated state
        await DbContext.Entry(order).ReloadAsync();

        // Assert
        order.State.Should().Be(OrderState.Paid);
    }
    
    [Fact]
    public async Task Handle_Should_Throw_When_Order_Not_Found()
    {
        await ResetDatabase();

        var command = new PayOrderCommand { Id = TestGuids.Guid3 };

        // Act
        var act = () => Sender.Send(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}