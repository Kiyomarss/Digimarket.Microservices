
using BuildingBlocks.CQRS;

namespace Identity.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : ICommand<Guid>;
