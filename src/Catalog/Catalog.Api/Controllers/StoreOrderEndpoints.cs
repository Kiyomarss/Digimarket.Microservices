using Carter;
using Catalog.Components;
using Catalog.Components.Contracts;

namespace Catalog.Api.Controllers;

public class StoreCatalogEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/StoreCatalog", async (
                        CatalogRequest request,
                        IProductService CatalogsService) =>
                    {
                        try
                        {
                            var Catalogs = await CatalogsService.SubmitCatalogs();

                            var result = new
                            {
                                RegistrationId = Catalogs.Id
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