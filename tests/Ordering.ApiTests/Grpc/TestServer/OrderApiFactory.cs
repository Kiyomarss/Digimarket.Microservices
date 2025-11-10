using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Grpc.Net.Client;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Ordering_Infrastructure.Data.DbContext;

public class OrderApiFactory : WebApplicationFactory<Program>
{
    public GrpcChannel GrpcChannel { get; private set; } = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // حذف DbContextOptions قبلی (Postgres) و استفاده از InMemory برای تست
            var descriptor = services.SingleOrDefault(
                                                      d => d.ServiceType == typeof(DbContextOptions<OrderingDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<OrderingDbContext>(o =>
            {
                o.UseInMemoryDatabase("Ordering_Db_For_Tests");
            });
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        // ایجاد GrpcChannel روی همان HttpClient که TestServer ساخته
        GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = client
        });
    }
}