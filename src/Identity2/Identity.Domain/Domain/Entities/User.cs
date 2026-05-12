using System.Security.Cryptography;

namespace Identity.Domain.Domain.Entities;

public class User
{
    private readonly List<UserRole> _roles = new();

    private readonly List<UserClaim> _claims = new();

    private readonly List<RefreshToken> _refreshTokens = new();

    private User() { }

    public User(Guid id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public IReadOnlyCollection<UserClaim> Claims => _claims;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public void AddRole(Guid roleId)
    {
        if (_roles.Any(x => x.RoleId == roleId))

            return;

        _roles.Add(new UserRole(Id, roleId));
    }

    public void AddClaim(string type, string value)
    {
        _claims.Add(new UserClaim(Id, type, value));
    }

    public RefreshToken CreateRefreshToken()
    {
        var token = GenerateToken();

        var refreshToken = new RefreshToken(
                                            Guid.NewGuid(),
                                            Id,
                                            token,
                                            DateTime.UtcNow.AddDays(7));

        _refreshTokens.Add(refreshToken);

        return refreshToken;
    }

    private static string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }
}