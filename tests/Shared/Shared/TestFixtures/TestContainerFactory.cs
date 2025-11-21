using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Shared.TestFixtures
{
    public static class TestContainerFactory
    {
        public static IContainer CreatePostgresContainer(string dbName = "OrderingDb", string username = "postgres", string password = "123", int port = 5432)
        {
            return new ContainerBuilder()
                   .WithImage("postgres:16")
                   .WithPortBinding(port, true)
                   .WithEnvironment("POSTGRES_USER", username)
                   .WithEnvironment("POSTGRES_PASSWORD", password)
                   .WithEnvironment("POSTGRES_DB", dbName)
                   .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(port))
                   .Build();
        }

        public static IContainer CreateRabbitMqContainer(int amqpPort = 5672, int managementPort = 15672, string user = "guest", string pass = "guest")
        {
            return new ContainerBuilder()
                   .WithImage("rabbitmq:3-management")
                   .WithPortBinding(amqpPort, true)
                   .WithPortBinding(managementPort, true)
                   .WithEnvironment("RABBITMQ_DEFAULT_USER", user)
                   .WithEnvironment("RABBITMQ_DEFAULT_PASS", pass)
                   .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(amqpPort))
                   .Build();
        }
    }
}