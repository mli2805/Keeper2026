using KeeperDomain;

namespace KeeperInfrastructure;

public class ToSqlite(KeeperDbContext keeperDbContext)
{

    public async Task SaveModelToDb(KeeperModel keeperModel)
    {
        // Disable change tracking for bulk inserts
        keeperDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        
        // Use AddRange instead of individual Add calls
        keeperDbContext.Accounts.AddRange(keeperModel.AccountPlaneList.Select(item => item.ToEf()));
        keeperDbContext.BankAccounts.AddRange(keeperModel.BankAccounts.Select(item => item.ToEf()));
        keeperDbContext.Deposits.AddRange(keeperModel.Deposits.Select(item => item.ToEf()));
        keeperDbContext.PayCards.AddRange(keeperModel.PayCards.Select(item => item.ToEf()));
        keeperDbContext.TrustAccounts.AddRange(keeperModel.TrustAccounts.Select(item => item.ToEf()));
        keeperDbContext.TrustAssets.AddRange(keeperModel.TrustAssets.Select(item => item.ToEf()));
        keeperDbContext.TrustAssetRates.AddRange(keeperModel.TrustAssetRates.Select(item => item.ToEf()));
        keeperDbContext.TrustTransactions.AddRange(keeperModel.TrustTransactions.Select(item => item.ToEf()));
        keeperDbContext.OfficialRates.AddRange(keeperModel.OfficialRates.Select(item => item.ToEf()));
        keeperDbContext.ExchangeRates.AddRange(keeperModel.ExchangeRates.Select(item => item.ToEf()));
        keeperDbContext.RefinancingRates.AddRange(keeperModel.RefinancingRates.Select(item => item.ToEf()));
        keeperDbContext.MetalRates.AddRange(keeperModel.MetalRates.Select(item => item.ToEf()));
        keeperDbContext.Cars.AddRange(keeperModel.Cars.Select(item => item.ToEf()));
        keeperDbContext.CarYearMileages.AddRange(keeperModel.CarYearMileages.Select(item => item.ToEf()));
        keeperDbContext.DepositOffers.AddRange(keeperModel.DepositOffers.Select(item => item.ToEf()));
        keeperDbContext.DepositConditions.AddRange(keeperModel.DepositConditions.Select(item => item.ToEf()));
        keeperDbContext.DepositRateLines.AddRange(keeperModel.DepositRateLines.Select(item => item.ToEf()));
        keeperDbContext.Transactiones.AddRange(keeperModel.Transactions.Select(item => item.ToEf()));
        keeperDbContext.Fuellings.AddRange(keeperModel.Fuellings.Select(item => item.ToEf()));
        keeperDbContext.SalaryChanges.AddRange(keeperModel.SalaryChanges.Select(item => item.ToEf()));
        keeperDbContext.LargeExpenseThresholds.AddRange(keeperModel.LargeExpenseThresholds.Select(item => item.ToEf()));
        keeperDbContext.CardBalanceMemos.AddRange(keeperModel.CardBalanceMemos.Select(item => item.ToEf()));
        keeperDbContext.ButtonCollections.AddRange(keeperModel.ButtonCollections.Select(item => item.ToEf()));

        await keeperDbContext.SaveChangesAsync();
        
        // Re-enable change tracking
        keeperDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
