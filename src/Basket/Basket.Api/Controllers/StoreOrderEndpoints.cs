using Carter;
using Basket.Components;
using Basket.Components.Contracts;

namespace Basket.Api.Controllers;

public class StoreBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/StoreBasket", async (
                        BasketRequest request,
                        IShoppingCartService BasketsService) =>
                    {
                        try
                        {
                            var Baskets = await BasketsService.SubmitBaskets();

                            var result = new
                            {
                                RegistrationId = Baskets.Id
                            };

                            return Results.Ok(result);
                        }
                        catch (DuplicateProductException)
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