using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Identity.Authorization;

public class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission) : base(policy: $"Permission:{permission}") { }
}