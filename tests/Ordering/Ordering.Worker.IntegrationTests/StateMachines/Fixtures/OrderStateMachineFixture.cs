using MassTransit;
using MassTransit.Testing;
using Ordering.Worker.Configurations.Saga;
using Ordering.Worker.StateMachines;

namespace Ordering.Worker.IntegrationTests.StateMachines.Fixtures
{
    public class OrderStateMachineFixture : IAsyncDisposable
    {
        public InMemoryTestHarness Harness { get; }
        public ISagaStateMachineTestHarness<OrderStateMachine, OrderState> SagaHarness { get; }
        private bool _started;

        public OrderStateMachineFixture()
        {
            Harness = new InMemoryTestHarness();

            // تنظیم Message Scheduler برای تست پیام‌های زمان‌بندی‌شده
            Harness.OnConfigureInMemoryBus += configurator =>
            {
                configurator.UseMessageScheduler(new Uri("queue:quartz"));
            };

            var machine = new OrderStateMachine();
            var repository = new InMemorySagaRepository<OrderState>();

            SagaHarness = Harness.StateMachineSaga(machine, repository);
        }

        public async Task StartAsync()
        {
            if (_started)
                return;

            _started = true;
            await Harness.Start();
        }

        public async ValueTask DisposeAsync()
        {
            if (Harness != null)
                await Harness.Stop();
        }
    }
}