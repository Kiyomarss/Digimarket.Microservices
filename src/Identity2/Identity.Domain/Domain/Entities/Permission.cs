namespace Identity.Domain.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; set; }
}