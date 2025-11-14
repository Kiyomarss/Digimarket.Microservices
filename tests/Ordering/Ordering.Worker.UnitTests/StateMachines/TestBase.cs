using MassTransit;
using MassTransit.Testing;

public abstract class TestBase : IAsyncLifetime
{
    protected InMemoryTestHarness Harness;

    public virtual Task InitializeAsync()
    {
        Harness = new InMemoryTestHarness();

        // این بسیار مهم است:
        Harness.OnConfigureInMemoryBus += configurator =>
        {
            configurator.UseInMemoryScheduler();
        };

        return Harness.Start();
    }

    public virtual Task DisposeAsync()
    {
        return Harness.Stop();
    }
}