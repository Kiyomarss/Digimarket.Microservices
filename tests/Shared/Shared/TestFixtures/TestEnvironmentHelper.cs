using DotNet.Testcontainers.Containers;

namespace Shared.TestFixtures;
public static class TestEnvironmentHelper
{
    public static string PostgresConnectionString { get; private set; } = default!;

    public static void SetPostgresConnectionString(IContainer postgresContainer)
    {
        PostgresConnectionString =
            $"Host=localhost;Port={postgresContainer.GetMappedPublicPort(5432)};" +
            $"Database=ordering_test_{Guid.NewGuid():N};" +
            $"Username=postgres;Password=123;";
        
        Environment.SetEnvironmentVariable(
                                           "DATABASE_CONNECTION_STRING",
                                           PostgresConnectionString);
    }
}