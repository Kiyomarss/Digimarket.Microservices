namespace Identity.Domain.Domain.Entities;

public class UserClaim
{
    private UserClaim() { }

    public UserClaim(Guid userId, string type, string value)
    {
        UserId = userId;
        Type = type;
        Value = value;
    }

    public Guid UserId { get; private set; }
    
    public User User { get; private set; }

    public string Type { get; private set; }

    public string Value { get; private set; }
}