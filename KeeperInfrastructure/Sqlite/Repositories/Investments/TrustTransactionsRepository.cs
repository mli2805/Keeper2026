using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class TrustTransactionsRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<TrustTranModel>> GetAllTrustTransactionModels(Dictionary<int, AccountItemModel> acMoDict,
           List<TrustAccount> trustAccounts, List<TrustAssetModel> trustAssetModels)
    {
        var transactionsEf = await keeperDbContext.TrustTransactions.ToListAsync();
        var result = new List<TrustTranModel>(transactionsEf.Count);
        foreach (var transaction in transactionsEf)
        {
            var transactionModel = new TrustTranModel
            {
                Id = transaction.Id,
                InvestOperationType = transaction.InvestOperationType,
                Timestamp = transaction.Timestamp,
                AccountItemModel = transaction.AccountId != 0 ? acMoDict[transaction.AccountId] : null,
                TrustAccount = trustAccounts.FirstOrDefault(t => t.Id == transaction.TrustAccountId),
                CurrencyAmount = transaction.CurrencyAmount,
                CouponAmount = transaction.CouponAmount,
                Currency = transaction.Currency,
                AssetAmount = transaction.AssetAmount,
                Asset = trustAssetModels.FirstOrDefault(a => a.Id == transaction.AssetId),
                BuySellFee = transaction.PurchaseFee,
                BuySellFeeCurrency = transaction.PurchaseFeeCurrency == 0 ? CurrencyCode.BYN : transaction.PurchaseFeeCurrency,
                FeePaymentOperationId = transaction.FeePaymentOperationId,
                Comment = transaction.Comment,
            };
            result.Add(transactionModel);
        }
        return result;
    }

    public List<TrustTransaction> GetAllTrustTransactions()
    {
        var result = keeperDbContext.TrustTransactions.Select(tt => tt.FromEf()).ToList();
        return result;
    }
}
