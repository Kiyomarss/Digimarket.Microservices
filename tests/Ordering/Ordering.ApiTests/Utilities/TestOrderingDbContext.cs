using Microsoft.EntityFrameworkCore;
using Ordering_Infrastructure.Data.DbContext;

namespace Ordering.ApiTests.Utilities;

public class TestOrderingDbContext : OrderingDbContext
{
    public TestOrderingDbContext(DbContextOptions<OrderingDbContext> options)
        : base(options)
    {
    }
}