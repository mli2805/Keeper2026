using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustAssetRatesRepository(KeeperDbContext keeperDbContext)
{
    public List<TrustAssetRate> GetAllTrustAssetRates()
    {
        var result = keeperDbContext.TrustAssetRates.Select(tar => tar.FromEf()).ToList();
        return result;
    }
}
