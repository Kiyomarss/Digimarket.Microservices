using Identity.Application.RepositoryContracts;
using Identity.Domain.Domain.Entities;
using Identity.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }


    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _context.Users
                             .Include(u => u.UserRoles)
                             .ThenInclude(ur => ur.Role)
                             .ThenInclude(x => x.RolePermissions)
                             .ThenInclude(x => x.Permission)
                             .Include(u => u.UserClaims)
                             .AsSplitQuery()
                             .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _context.Users
                       .Include(x => x.UserRoles)
                       .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}