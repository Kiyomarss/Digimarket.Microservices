using System;
using Grpc.Core;

namespace BuildingBlocks.Services;

public interface ICurrentUserService
{
    Guid? GetUserId();
    Guid GetRequiredUserId();
    string? GetUserName();
    string? GetEmail();
    bool IsAuthenticated();
    Metadata GetAuthorizationHeaders();
}