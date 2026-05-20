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
    
    public void AddUser(Guid userId)
    {
        _userRoles.Add(new UserRole(userId, Id));
    }
}