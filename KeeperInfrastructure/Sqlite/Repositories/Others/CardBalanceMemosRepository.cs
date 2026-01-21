using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class CardBalanceMemosRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<CardBalanceMemoModel>> GetAllCardBalanceMemos(Dictionary<int, AccountItemModel> AcMoDict)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = await keeperDbContext.CardBalanceMemos
            .Select(cm => new CardBalanceMemoModel
            {
                Id = cm.Id,
                Account = AcMoDict[cm.AccountId],
                BalanceThreshold = cm.BalanceThreshold,
            }).ToListAsync();
        return result;
    }

    public List<CardBalanceMemo> GetAllCardBalanceMemos()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.CardBalanceMemos.Select(cm => cm.FromEf()).ToList();
        return result;
    }
}
