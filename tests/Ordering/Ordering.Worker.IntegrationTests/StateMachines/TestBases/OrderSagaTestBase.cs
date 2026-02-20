using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines;
using Ordering.Worker.IntegrationTests.StateMachines.Fixtures;

namespace Ordering.Worker.IntegrationTests.StateMachines.TestBases
{
    public abstract class OrderSagaTestBase :
        IClassFixture<OrderSagaFixture>,
        IAsyncLifetime
    {
        protected readonly OrderSagaFixture Fixture;

        protected InMemoryTestHarness Harness => Fixture.Harness;
        protected ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness => Fixture.SagaHarness;
        protected OrderStateMachine Machine => SagaHarness.StateMachine;

        protected OrderSagaTestBase(OrderSagaFixture fixture)
        {
            Fixture = fixture;
        }

        public Task InitializeAsync() => Fixture.StartAsync();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}