using KeeperDomain;

namespace KeeperInfrastructure;

public class TrustAccountsRepository(KeeperDbContext keeperDbContext)
{
    public List<TrustAccount> GetAllTrustAccounts()
    {
        var result = keeperDbContext.TrustAccounts.Select(ta => ta.FromEf()).ToList();
        return result;
    }
}

public class TrustAssetsRepository(KeeperDbContext keeperDbContext)
{
    public List<TrustAsset> GetAllTrustAssets()
    {
        var result = keeperDbContext.TrustAssets.Select(ta => ta.FromEf()).ToList();
        return result;
    }
}

public class TrustAssetRatesRepository(KeeperDbContext keeperDbContext)
{
    public List<TrustAssetRate> GetAllTrustAssetRates()
    {
        var result = keeperDbContext.TrustAssetRates.Select(tar => tar.FromEf()).ToList();
        return result;
    }
}

public class TrustTransactionsRepository(KeeperDbContext keeperDbContext)
{
    public List<TrustTransaction> GetAllTrustTransactions()
    {
        var result = keeperDbContext.TrustTransactions.Select(tt => tt.FromEf()).ToList();
        return result;
    }
}
