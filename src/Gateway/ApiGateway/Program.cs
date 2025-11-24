using ApiGateway;

var builder = WebApplication.CreateBuilder(args);

// YARP
builder.Services
       .AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Required for Swagger (this registers the API explorer services)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var swaggerServices = builder.Configuration.GetSection("SwaggerServices").Get<List<SwaggerService>>() 
                      ?? new List<SwaggerService>();

var app = builder.Build();

// Swagger UI (Gateway will aggregate external swagger.json entries if configured)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // اگر در appsettings بخشی برای SwaggerServices نداری، این حلقه امن است (هیچ endpoint نخواهد ساخت)
    foreach (var svc in swaggerServices)
    {
        // svc.SwaggerEndpoint باید آدرس کامل swagger.json سرویس مقصد باشد
        c.SwaggerEndpoint(svc.SwaggerEndpoint, svc.Name);
    }
});

// Map YARP proxy routes (appsettings "ReverseProxy")
app.MapReverseProxy();

app.Run();

namespace ApiGateway
{
    public class SwaggerService
    {
        public string Name { get; set; } = default!;
        public string SwaggerEndpoint { get; set; } = default!;
    }
}