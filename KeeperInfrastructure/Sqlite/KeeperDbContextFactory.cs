using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure.Sqlite;

public class KeeperDbContextFactory : IDbContextFactory<KeeperDbContext>
{
    private readonly DbContextOptions<KeeperDbContext> _options;

    public KeeperDbContextFactory(DbContextOptions<KeeperDbContext> options)
    {
        _options = options;
    }

    public KeeperDbContext CreateDbContext()
    {
        return new KeeperDbContext(_options);
    }
}

