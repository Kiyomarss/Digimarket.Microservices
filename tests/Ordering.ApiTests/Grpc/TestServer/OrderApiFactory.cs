using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Net.Client;
using System.Net.Http;
using Ordering_Infrastructure.Data.DbContext;
using Ordering.ApiTests.Utilities;

public class OrderingApiFactory : WebApplicationFactory<Program>
{
    public GrpcChannel GrpcChannel { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // فعال کردن HTTP/2 برای gRPC
        builder.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(o =>
            {
                o.Protocols = HttpProtocols.Http2;
            });
        });

        builder.ConfigureServices(services =>
        {
            // حذف MassTransit و جایگزین با Fake
            services.RemoveMassTransitForTests();

            // جایگزینی DbContext با نسخه InMemory مخصوص تست
            services.ReplaceDbContextWithInMemory("OrderingTestDb");

            // Fake gRPC Client برای ProductGrpc
            services.AddSingleton<ProductGrpc.ProductProtoService.ProductProtoServiceClient, FakeProductGrpcClient>();
        });
        
        builder.UseSetting("UseInMemory", "true");
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        // ساخت gRPC Channel از روی HttpClient خود WebApplicationFactory
        GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = client,
            MaxReceiveMessageSize = null,
            MaxSendMessageSize = null
        });
    }
}