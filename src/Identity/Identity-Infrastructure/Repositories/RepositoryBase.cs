using RepositoryContracts;

namespace Hesabdari_Infrastructure.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly IApplicationDbContext _db;

        public RepositoryBase(IApplicationDbContext db)
        {
            _db = db;
        }
    }
}