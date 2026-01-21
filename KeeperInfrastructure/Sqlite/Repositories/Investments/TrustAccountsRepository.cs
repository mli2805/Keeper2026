using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustAccountsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TrustAccount>> GetAllTrustAccounts()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = await keeperDbContext.TrustAccounts.Select(ta => ta.FromEf()).ToListAsync();
        return result;
    }
}
