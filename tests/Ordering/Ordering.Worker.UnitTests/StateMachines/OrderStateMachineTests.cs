using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines;
using Shared.IntegrationEvents.Ordering;

public class OrderStateMachineTests : TestBase
{
    [Fact]
    public async Task ShouldConsumeOrderInitiated()
    {
        var machine = new OrderStateMachine();
        var repo = new InMemorySagaRepository<OrderState>();

        var saga = Harness.StateMachineSaga(machine, repo);

        var orderId = Guid.NewGuid();

        await Harness.Bus.Publish(new OrderInitiated());

        Assert.True(await saga.Created.Any(x => x.CorrelationId == orderId));
    }
}