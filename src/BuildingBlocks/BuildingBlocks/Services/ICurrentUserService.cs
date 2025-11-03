using System;

namespace BuildingBlocks.Services;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserName();
    string? GetEmail();
    bool IsAuthenticated();
}