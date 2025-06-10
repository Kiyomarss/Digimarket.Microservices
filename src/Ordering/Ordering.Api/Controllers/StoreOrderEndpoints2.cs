using Carter;
using Ordering.Components;

namespace Ordering.Api.Controllers;

public class StoreOrderEndpoints2 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/StoreOrder2", async (Guid orderId, IOrderService ordersService) =>
                    {
                        await ordersService.SubmitOrders2(orderId);

                        return Results.Ok(orderId);
                    })
           .WithName("SubmitRegisyuyutration")
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status409Conflict)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithSummary("Submit Registration")
           .WithDescription("Registers a member for an event.");
    }
}