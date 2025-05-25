using Carter;
using Ordering.Components;

namespace Ordering.Api.Controllers;

public record OrderRequest(List<OrderItem> OrderItems);

public class StoreOrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/StoreOrder", async (
                        OrderRequest request,
                        IOrderService ordersService) =>
                    {
                        try
                        {
                            var orders = await ordersService.SubmitOrders(request.OrderItems);

                            var result = new
                            {
                                RegistrationId = orders.Id,
                                RegistrationDate = orders.Date,
                                orders.Customer,
                            };

                            return Results.Ok(result);
                        }
                        catch (DuplicateOrderException)
                        {
                            return Results.Conflict(new
                            {
                                Customer = "kiyomarss",
                            });
                        }
                    })
           .WithName("SubmitRegistration")
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithSummary("Submit Registration")
           .WithDescription("Registers a member for an event.");
    }
}