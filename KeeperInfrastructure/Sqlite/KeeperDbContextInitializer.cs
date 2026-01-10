namespace KeeperInfrastructure;

public class KeeperDbContextInitializer(KeeperDbContext keeperDbContext)
{
    public async Task InitializeAsync()
    {
        // при подкладывании файлов из Keeper2018 удалить базу перед созданием 
         //await keeperDbContext.Database.EnsureDeletedAsync();
        await keeperDbContext.Database.EnsureCreatedAsync();
    }
}
