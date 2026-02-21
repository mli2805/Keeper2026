using KeeperInfrastructure;
using KeeperModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Keeper;

public static class DbTestHelper
{
    private static SqliteConnection? _templateConnection;
    private static readonly object _lock = new();

    public static Dictionary<int, AccountItemModel> AcMoDict { get; private set; } = new();

    public static async Task InitializeTemplateAsync()
    {
        if (_templateConnection != null)
            return;

        lock (_lock)
        {
            if (_templateConnection != null)
                return;

            _templateConnection = new SqliteConnection("DataSource=:memory:");
            _templateConnection.Open();
        }

        var options = new DbContextOptionsBuilder<KeeperDbContext>()
            .UseSqlite(_templateConnection)
            .Options;

        await using var ctx = new KeeperDbContext(options);
        await ctx.Database.EnsureCreatedAsync();

        var keeperDomainModel =
            await TxtLoader.LoadAllFromTextFiles(@"..\..\..\TxtFiles");

        var toSqlite = new ToSqlite(new TestDbContextFactory(options));
        await toSqlite.SaveModelToDb(keeperDomainModel!);

        var accountRepository = new AccountRepository(new TestDbContextFactory(options));
        var pair = await accountRepository.GetAccountModelsTreeAndDict();
        AcMoDict = pair?.Item2 ?? new Dictionary<int, AccountItemModel>();
    }

    public static IDbContextFactory<KeeperDbContext> CreateIsolatedFactory()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        _templateConnection!.BackupDatabase(connection);

        var options = new DbContextOptionsBuilder<KeeperDbContext>()
            .UseSqlite(connection)
            .Options;

        return new TestDbContextFactory(options);
    }

    private class TestDbContextFactory : IDbContextFactory<KeeperDbContext>
    {
        private readonly DbContextOptions<KeeperDbContext> _options;

        public TestDbContextFactory(DbContextOptions<KeeperDbContext> options)
        {
            _options = options;
        }

        public KeeperDbContext CreateDbContext()
        {
            return new KeeperDbContext(_options);
        }
    }
}
