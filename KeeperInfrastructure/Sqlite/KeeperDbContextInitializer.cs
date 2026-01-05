namespace KeeperInfrastructure
{
    public class KeeperDbContextInitializer(KeeperDbContext keeperDbContext)
    {
        public async Task InitializeAsync()
        {
            await keeperDbContext.Database.EnsureCreatedAsync();
        }
    }
}
