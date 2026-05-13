using Identity.Domain.Domain.Entities;

namespace Identity.Application.RepositoryContracts;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id);

    void Update(User user);
}