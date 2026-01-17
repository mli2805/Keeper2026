using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustAccountsRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<TrustAccount>> GetAllTrustAccounts()
    {
        var result = await keeperDbContext.TrustAccounts.Select(ta => ta.FromEf()).ToListAsync();
        return result;
    }
}
