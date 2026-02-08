using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TransactionsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TransactionModel>> GetAllTransactionModels(Dictionary<int, AccountItemModel> acMoDict)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var transactionsEf = await keeperDbContext.Transactions
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
        var result = transactionsEf.Select(transactionsEf => transactionsEf.FromEf(acMoDict)).ToList();
        return result;
    }

    public async Task AddTransactions(List<TransactionModel> transactionModels)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var transactionsEf = transactionModels.Select(tm => tm.ToEf()).ToList();
        await keeperDbContext.Transactions.AddRangeAsync(transactionsEf);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task UpdateTransaction(TransactionModel transactionModel)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var transactionEf = await keeperDbContext.Transactions.FirstAsync(t => t.Id == transactionModel.Id);
        transactionEf.UpdateEf(transactionModel);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteTransactions(List<int> transactionsId)
    {
        using var keeperDbContext = factory.CreateDbContext();
        await keeperDbContext.Transactions
            .Where(t => transactionsId.Contains(t.Id))
            .ExecuteDeleteAsync();
    }
}


