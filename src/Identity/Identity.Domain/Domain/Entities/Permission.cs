namespace Identity.Domain.Domain.Entities;

public class Permission
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public string Name { get; private set; }
}