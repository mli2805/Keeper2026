using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class ToSqlite(IDbContextFactory<KeeperDbContext> factory)
{
    // данные в формате старого Keeper2018, вычитанные из текстовых файлов, сохраняем в БД SQLite
    public async Task SaveModelToDb(KeeperDomainModel keeperDomainModel)
    {
        using var keeperDbContext = factory.CreateDbContext();

        // Disable change tracking for bulk inserts
        keeperDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        
        // Use AddRange instead of individual Add calls
        keeperDbContext.Accounts.AddRange(keeperDomainModel.AccountPlaneList.Select(item => item.ToEf()));
        keeperDbContext.BankAccounts.AddRange(keeperDomainModel.BankAccounts.Select(item => item.ToEf()));
        keeperDbContext.Deposits.AddRange(keeperDomainModel.Deposits.Select(item => item.ToEf()));
        keeperDbContext.PayCards.AddRange(keeperDomainModel.PayCards.Select(item => item.ToEf()));
        keeperDbContext.TrustAccounts.AddRange(keeperDomainModel.TrustAccounts.Select(item => item.ToEf()));
        keeperDbContext.TrustAssets.AddRange(keeperDomainModel.TrustAssets.Select(item => item.ToEf()));
        keeperDbContext.TrustAssetRates.AddRange(keeperDomainModel.TrustAssetRates.Select(item => item.ToEf()));
        keeperDbContext.TrustTransactions.AddRange(keeperDomainModel.TrustTransactions.Select(item => item.ToEf()));
        keeperDbContext.OfficialRates.AddRange(keeperDomainModel.OfficialRates.Select(item => item.ToEf()));
        keeperDbContext.ExchangeRates.AddRange(keeperDomainModel.ExchangeRates.Select(item => item.ToEf()));
        keeperDbContext.RefinancingRates.AddRange(keeperDomainModel.RefinancingRates.Select(item => item.ToEf()));
        keeperDbContext.MetalRates.AddRange(keeperDomainModel.MetalRates.Select(item => item.ToEf()));
        keeperDbContext.Cars.AddRange(keeperDomainModel.Cars.Select(item => item.ToEf()));
        keeperDbContext.CarYearMileages.AddRange(keeperDomainModel.CarYearMileages.Select(item => item.ToEf()));
        keeperDbContext.DepositOffers.AddRange(keeperDomainModel.DepositOffers.Select(item => item.ToEf()));
        keeperDbContext.DepositConditions.AddRange(keeperDomainModel.DepositConditions.Select(item => item.ToEf()));
        keeperDbContext.DepositRateLines.AddRange(keeperDomainModel.DepositRateLines.Select(item => item.ToEf()));
        keeperDbContext.Transactions.AddRange(keeperDomainModel.Transactions.Select(item => item.ToEf()));
        keeperDbContext.Fuellings.AddRange(keeperDomainModel.Fuellings.Select(item => item.ToEf()));
        keeperDbContext.SalaryChanges.AddRange(keeperDomainModel.SalaryChanges.Select(item => item.ToEf()));
        keeperDbContext.LargeExpenseThresholds.AddRange(keeperDomainModel.LargeExpenseThresholds.Select(item => item.ToEf()));
        keeperDbContext.CardBalanceMemos.AddRange(keeperDomainModel.CardBalanceMemos.Select(item => item.ToEf()));
        keeperDbContext.ButtonCollections.AddRange(keeperDomainModel.ButtonCollections.Select(item => item.ToEf()));

        await keeperDbContext.SaveChangesAsync();
        
        // Re-enable change tracking
        keeperDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
    }
}
