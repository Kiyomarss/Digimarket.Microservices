namespace Identity.Domain.Domain.Entities;

public class Role
{
    private Role() { }

    public Role(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }
    
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
    
    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;
    
    public void AddUser(Guid userId)
    {
        _userRoles.Add(new UserRole(userId, Id));
    }
    
    public void AddPermission(Guid permissionId)
    {
        _rolePermissions.Add(new RolePermission(Id, permissionId));
    }

}