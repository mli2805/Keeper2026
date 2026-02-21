using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class KeeperDbContextInitializer(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task InitializeAsync()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();

        // при подкладывании файлов из Keeper2018 удалить базу перед созданием 
        //await keeperDbContext.Database.EnsureDeletedAsync();
        await keeperDbContext.Database.EnsureCreatedAsync();
    }
}
