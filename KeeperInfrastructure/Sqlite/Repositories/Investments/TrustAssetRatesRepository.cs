using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustAssetRatesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<TrustAssetRate> GetAllTrustAssetRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.TrustAssetRates.Select(tar => tar.FromEf()).ToList();
        return result;
    }
}
