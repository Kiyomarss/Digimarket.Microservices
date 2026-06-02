namespace Identity.Domain.Domain.Entities;

public class RefreshToken
{
    private RefreshToken() { }

    public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public Guid UserId { get; private set; }
    
    public User User { get; private set; }

    public string Token { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
    }
}