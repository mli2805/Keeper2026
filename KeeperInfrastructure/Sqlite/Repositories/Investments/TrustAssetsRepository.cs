using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustAssetsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TrustAssetModel>> GetAllTrustAssetModels(List<TrustAccount> trustAccounts)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var assetsEf = await keeperDbContext.TrustAssets.ToListAsync();
        var result = new List<TrustAssetModel>(assetsEf.Count);
        foreach (var asset in assetsEf)
        {
           
            var assetModel = new TrustAssetModel
            {
                Id = asset.Id,
                TrustAccount = trustAccounts.First(t => t.Id == asset.TrustAccountId),
                Ticker = asset.Ticker,
                Title = asset.Title,
                StockMarket = asset.StockMarket,
                AssetType = asset.AssetType,
                Nominal = asset.Nominal ?? 0,
                BondCouponPeriod = asset.AssetType == AssetType.Bond
                    ? new DurationModel((int)asset.BondCouponDurationValue!, (Durations)asset.BondCouponDuration!)
                    : new DurationModel(),
                CouponRate = asset.CouponRate ?? 0,
                PreviousCouponDate = asset.PreviousCouponDate ?? DateTime.MinValue,
                BondExpirationDate = asset.BondExpirationDate ?? DateTime.MinValue,
                Comment = asset.Comment,
            };
            result.Add(assetModel);
        }
        return result;
    }

    public List<TrustAsset> GetAllTrustAssets()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.TrustAssets.Select(ta => ta.FromEf()).ToList();
        return result;
    }
}
