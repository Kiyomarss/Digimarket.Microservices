using Grpc.Core;

namespace BuildingBlocks.Services;

public interface ICurrentUserService
{
    Guid? GetUserId();
    Task<Guid> GetRequiredUserId();
    string? GetUserName();
    bool IsAuthenticated();
    Metadata GetAuthorizationHeaders();
}