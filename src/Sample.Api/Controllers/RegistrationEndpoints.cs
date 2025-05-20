using Carter;
using Sample.Components;

namespace Sample.Api.Registration;

public record RegistrationRequest(string EventId, string MemberId, decimal Payment);

public class RegistrationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registration", async (
                        RegistrationRequest request,
                        IRegistrationService registrationService) =>
                    {
                        try
                        {
                            var registration = await registrationService.SubmitRegistration(request.EventId, request.MemberId, request.Payment);

                            var result = new
                            {
                                registration.RegistrationId,
                                registration.RegistrationDate,
                                registration.MemberId,
                                registration.EventId
                            };

                            return Results.Ok(result);
                        }
                        catch (DuplicateRegistrationException)
                        {
                            return Results.Conflict(new
                            {
                                request.MemberId,
                                request.EventId
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