using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class CardBalanceMemosRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<CardBalanceMemoModel>> GetAllCardBalanceMemos(Dictionary<int, AccountItemModel> AcMoDict)
    {
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
        var result = keeperDbContext.CardBalanceMemos.Select(cm => cm.FromEf()).ToList();
        return result;
    }
}
