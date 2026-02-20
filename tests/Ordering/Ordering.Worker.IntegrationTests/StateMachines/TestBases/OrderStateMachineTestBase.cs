using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines;
using Ordering.Worker.IntegrationTests.StateMachines.Fixtures;

namespace Ordering.Worker.IntegrationTests.StateMachines.TestBases
{
    public abstract class OrderStateMachineTestBase : IClassFixture<OrderStateMachineFixture>
    {
        protected readonly InMemoryTestHarness Harness;
        protected readonly ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness;
        protected readonly OrderStateMachine Machine;

        protected OrderStateMachineTestBase(OrderStateMachineFixture fixture)
        {
            Harness = fixture.Harness;
            SagaHarness = fixture.SagaHarness;
            Machine = SagaHarness.StateMachine;

            // شروع Harness در Fixture
            fixture.StartAsync().GetAwaiter().GetResult();
        }
    }
}