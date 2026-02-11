using BuildingBlocks.Exceptions;
using BuildingBlocks.Exceptions.Application;
using FluentAssertions;
using Ordering_Domain.Domain.Enum;
using Ordering.Application.Orders.Commands.PayOrder;
using Ordering.TestingInfrastructure.Fixtures;
using Ordering.TestingInfrastructure.TestBase;

namespace Ordering.Application.IntegrationTests.Orders.Queries.PayOrder;

public class PayOrderHandlerTests : OrderingAppTestBase
{
    public PayOrderHandlerTests(OrderingAppFactory fixture)
        : base(fixture) { }

    [Fact]
    public async Task Handle_Should_Set_Order_State_To_Paid()
    {
        await ResetDatabase();

        var order = new OrderBuilder()
                    .WithState(OrderState.Pending)
                    .WithItems((1, 100_000))
                    .Build();

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        // Act
        await Sender.Send(new PayOrderCommand { Id = order.Id });

        // Assert
        var updatedOrder = await DbContext.Orders.FindAsync(order.Id);

        updatedOrder!.State.Should().Be(OrderState.Paid);
    }
    
    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Order_Does_Not_Exist()
    {
        await ResetDatabase();

        var act = () => Sender.Send(new PayOrderCommand { Id = Guid.NewGuid() });

        await act.Should().ThrowAsync<NotFoundException>();
    }

}