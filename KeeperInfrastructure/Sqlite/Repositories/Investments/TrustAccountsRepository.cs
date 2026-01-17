using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class InvestmentsRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<TrustAssetModel>> GetAllTrustAssetsWithRatesFromDb()
    {
        var assetsEf = await keeperDbContext.TrustAssets
            .Include(a => a.Rates)
            .ToListAsync();

        var result = new List<TrustAssetModel>();
        foreach (var assetEf in assetsEf)
        {
            var assetModel = new TrustAssetModel
            {
                Id = assetEf.Id,
                TrustAccount = null, // Set this later if needed
                Ticker = assetEf.Ticker,
                Title = assetEf.Title,
                StockMarket = assetEf.StockMarket,
                AssetType = assetEf.AssetType,
                Nominal = assetEf.Nominal ?? 0,
                BondCouponPeriod = assetEf.AssetType == AssetType.Bond
                    ? new Duration(assetEf.BondCouponDurationValue!.Value, assetEf.BondCouponDuration!.Value)
                    : new Duration(),
                CouponRate = assetEf.CouponRate ?? 0,
                PreviousCouponDate = assetEf.PreviousCouponDate ?? DateTime.MinValue,
                BondExpirationDate = assetEf.BondExpirationDate ?? DateTime.MaxValue,
                Comment = assetEf.Comment
            };

            result.Add(assetModel);
        }

        return result;
    }
}


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
