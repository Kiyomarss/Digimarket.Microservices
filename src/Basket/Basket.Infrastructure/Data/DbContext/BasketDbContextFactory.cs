using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Basket.Infrastructure.Data.DbContext // مسیر دقیق DbContext خودتان
{
    public class BasketDbContextFactory : IDesignTimeDbContextFactory<BasketDbContext>
    {
        public BasketDbContext CreateDbContext(string[] args)
        {
            // پیدا کردن خودکار مسیر پروژه Basket.Api
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

            var apiProjectPath = Path.Combine(solutionDirectory, "src", "Basket", "Basket.Api");

            if (!Directory.Exists(apiProjectPath))
                throw new Exception($"Basket.Api project not found at: {apiProjectPath}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    $"Connection string 'DefaultConnection' not found or empty in {apiProjectPath}");

            var optionsBuilder = new DbContextOptionsBuilder<BasketDbContext>();
            optionsBuilder.UseNpgsql(connectionString, 
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("Basket.Infrastructure"));

            return new BasketDbContext(optionsBuilder.Options);
        }
    }
}