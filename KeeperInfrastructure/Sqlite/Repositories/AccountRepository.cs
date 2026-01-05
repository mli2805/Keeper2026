using KeeperDomain;

namespace KeeperInfrastructure;

public class AccountRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<Account>> GetAllAccounts()
    {
        return keeperDbContext.Accounts.Select(a=>a.FromEf()).ToList();
    }
}
