using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Ordering_Infrastructure.Data.DbContext // مسیر دقیق DbContext خودتان
{
    public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
    {
        public OrderingDbContext CreateDbContext(string[] args)
        {
            // پیدا کردن خودکار مسیر پروژه Ordering.Api
            var currentDirectory = Directory.GetCurrentDirectory();

            var solutionDirectory = currentDirectory;
            while (solutionDirectory != null && 
                   !File.Exists(Path.Combine(solutionDirectory, "Digimarket.Microservices.sln")) &&
                   !Directory.GetFiles(solutionDirectory, "*.sln").Any())
            {
                solutionDirectory = Directory.GetParent(solutionDirectory)?.FullName;
            }

            if (solutionDirectory == null)
                throw new Exception("Solution directory not found!");

            var apiProjectPath = Path.Combine(solutionDirectory, "src", "Ordering", "Ordering.Api");

            if (!Directory.Exists(apiProjectPath))
                throw new Exception($"Ordering.Api project not found at: {apiProjectPath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"Connection string 'DefaultConnection' not found or empty in {apiProjectPath}");

            var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();
            optionsBuilder.UseNpgsql(connectionString, 
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("Ordering.Infrastructure"));

            return new OrderingDbContext(optionsBuilder.Options);
        }
    }
}