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

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
                             .Include(x => x.Roles)
                             .Include(x => x.Claims)
                             .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
                             .Include(x => x.Roles)
                             .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}