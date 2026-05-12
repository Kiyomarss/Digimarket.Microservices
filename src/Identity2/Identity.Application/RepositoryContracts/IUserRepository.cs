using Identity.Domain.Domain.Entities;

namespace Identity.Application.RepositoryContracts;

public interface IUserRepository
{
    Task AddAsync(User user);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    void Update(User user);
}