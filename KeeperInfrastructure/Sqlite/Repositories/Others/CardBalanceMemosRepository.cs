using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class CardBalanceMemosRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<CardBalanceMemoModel>> GetAllCardBalanceMemos(Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = await keeperDbContext.CardBalanceMemos
            .Select(cm => new CardBalanceMemoModel
            {
                Id = cm.Id,
                Account = acMoDict[cm.AccountId],
                BalanceThreshold = cm.BalanceThreshold,
            }).ToListAsync();
        return result;
    }
}

[ExportRepository]
public class BankAccountMemosRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<BankAccountMemoModel>> GetAllBankAccountMemos(Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = keeperDbContext.BankAccountMemos.Select(cm => cm.ToModel(acMoDict)).ToList();
        return result;
    }

    public async Task SaveAll(List<BankAccountMemoModel> models)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entities = models.Select(m => m.ToEf()).ToList();
        keeperDbContext.BankAccountMemos.RemoveRange(keeperDbContext.BankAccountMemos);
        await keeperDbContext.BankAccountMemos.AddRangeAsync(entities);
        await keeperDbContext.SaveChangesAsync();
    }
}
