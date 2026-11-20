using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class TransactionsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TransactionModel>> GetAllTransactionModels(Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var transactionsEf = await keeperDbContext.Transactions
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
        var result = transactionsEf.Select(ef => ef.FromEf(acMoDict)).ToList();
        return result;
    }

    public async Task<TransactionModel> AddTransaction(
        TransactionModel transactionModel, Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var transactionEf = transactionModel.ToEf();
        await keeperDbContext.Transactions.AddAsync(transactionEf);
        await keeperDbContext.SaveChangesAsync();
        return transactionEf.FromEf(acMoDict);
    }

    public async Task<List<TransactionModel>> AddTransactions(
        List<TransactionModel> transactionModels, Dictionary<int, AccountItemModel> acMoDict)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var transactionsEf = transactionModels.Select(tm => tm.ToEf()).ToList();
        await keeperDbContext.Transactions.AddRangeAsync(transactionsEf);
        await keeperDbContext.SaveChangesAsync();
        var result = transactionsEf.Select(ef => ef.FromEf(acMoDict)).ToList();
        return result;
    }

    public async Task UpdateTransaction(TransactionModel transactionModel)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var transactionEf = await keeperDbContext.Transactions.FirstAsync(t => t.Id == transactionModel.Id);
        transactionEf.UpdateEf(transactionModel);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteTransactions(List<int> transactionsId)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        await keeperDbContext.Transactions
            .Where(t => transactionsId.Contains(t.Id))
            .ExecuteDeleteAsync();
    }
}


