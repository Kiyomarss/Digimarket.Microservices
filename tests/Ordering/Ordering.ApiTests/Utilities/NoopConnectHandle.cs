using MassTransit;

namespace Ordering.ApiTests.Utilities;

public class NoopConnectHandle : ConnectHandle
{
    public void Dispose() { }
    public void Disconnect() { }
}