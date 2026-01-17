using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TransactionsRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<TransactionModel>> GetAllTransactionModels(Dictionary<int, AccountItemModel> acMoDict)
    {
        var transactionsEf = await keeperDbContext.Transactions.ToListAsync();
        var result = new List<TransactionModel>(transactionsEf.Count);
        foreach (var t in transactionsEf)
        {
            var tagIds = string.IsNullOrWhiteSpace(t.Tags) ? new List<int>() : t.Tags.Split('|').Select(s => int.Parse(s.Trim())).ToList();
            var transactionModel = new TransactionModel()
            {
                Id = t.Id,
                Timestamp = t.Timestamp,
                Receipt = t.Receipt,
                Operation = t.Operation,
                PaymentWay = t.PaymentWay,
                MyAccount = acMoDict[t.MyAccount],
                MySecondAccount = t.MySecondAccount == null ? null : acMoDict[t.MySecondAccount!.Value],
                Counterparty = t.Counterparty == null ? null : acMoDict[t.Counterparty!.Value],
                Category = t.Category == null ? null : acMoDict[t.Category!.Value],
                Amount = t.Amount,
                AmountInReturn = t.AmountInReturn ?? 0,
                Currency = t.Currency,
                CurrencyInReturn = t.CurrencyInReturn,
                Tags = tagIds.Select(i=> acMoDict[i]).ToList(),
                Comment = t.Comment,
            };
            result.Add(transactionModel);
        }
        return result;
    }
   
    public List<Transaction> GetAllTransactions()
    {
        var result = keeperDbContext.Transactions.Select(t => t.FromEf()).ToList();
        return result;

    }
}

public class FuellingsRepository(KeeperDbContext keeperDbContext)
{
    public List<Fuelling> GetAllFuellings()
    {
        var result = keeperDbContext.Fuellings.Select(f => f.FromEf()).ToList();
        return result;
    }
}
