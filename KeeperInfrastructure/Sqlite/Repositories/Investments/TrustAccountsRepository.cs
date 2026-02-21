using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustAccountsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TrustAccount>> GetAllTrustAccounts()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = await keeperDbContext.TrustAccounts.Select(ta => ta.FromEf()).ToListAsync();
        return result;
    }
}
