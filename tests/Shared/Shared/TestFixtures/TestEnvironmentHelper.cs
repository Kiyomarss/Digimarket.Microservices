using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Shared.TestFixtures
{
    public static class TestEnvironmentHelper
    {
        public static void SetPostgresConnectionString(IContainer postgresContainer)
        {
            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING",
                                               $"Host=localhost;Port={postgresContainer.GetMappedPublicPort(5432)};Database=OrderingDb;Username=postgres;Password=123;");
        }

        public static void SetRabbitMqHost(IContainer rabbitMqContainer)
        {
            Environment.SetEnvironmentVariable("RABBITMQ_HOST",
                                               $"localhost:{rabbitMqContainer.GetMappedPublicPort(5672)}");
        }
    }
}