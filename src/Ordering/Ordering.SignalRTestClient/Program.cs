using Microsoft.AspNetCore.SignalR.Client;

var orderId = Guid.Parse("11111111-1111-1111-1111-111111111111");

var connection = new HubConnectionBuilder()
                 .WithUrl("https://localhost:5001/hubs/orders", options =>
                 {
                     options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                     {
                         ServerCertificateCustomValidationCallback =
                             HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                     };
                 })
                 .WithAutomaticReconnect()
                 .Build();

connection.On<object>("OrderStatusUpdated", data =>
{
    Console.WriteLine($"Order update received: {System.Text.Json.JsonSerializer.Serialize(data)}");
});

connection.On<string>("Ping", message =>
{
    Console.WriteLine($"Received: {message}");
});

await connection.StartAsync();
Console.WriteLine("Connection started.");

// ✅ این خط خیلی مهم است
await connection.InvokeAsync("JoinOrderGroup", orderId);
Console.WriteLine("Joined group.");

Console.ReadLine();