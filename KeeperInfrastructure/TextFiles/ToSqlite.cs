using KeeperDomain;

namespace KeeperInfrastructure;

public class ToSqlite(KeeperDbContext keeperDbContext)
{

    public async Task SaveModelToDb(KeeperModel keeperModel)
    {
        foreach (var item in keeperModel.AccountPlaneList)
        {
            keeperDbContext.Accounts.Add(item.ToEf());
        }
        foreach (var item in keeperModel.BankAccounts)
        {
            keeperDbContext.BankAccounts.Add(item.ToEf());
        }
        foreach (var item in keeperModel.Deposits)
        {
            keeperDbContext.Deposits.Add(item.ToEf());
        }
        foreach (var item in keeperModel.PayCards)
        {
            keeperDbContext.PayCards.Add(item.ToEf());
        }

        foreach (var item in keeperModel.OfficialRates)
        {
            keeperDbContext.OfficialRates.Add(item.ToEf());
        }
        foreach (var item in keeperModel.ExchangeRates)
        {
            keeperDbContext.ExchangeRates.Add(item.ToEf());
        }
        foreach (var item in keeperModel.RefinancingRates)
        {
            keeperDbContext.RefinancingRates.Add(item.ToEf());
        }
        foreach (var item in keeperModel.MetalRates)
        {
            keeperDbContext.MetalRates.Add(item.ToEf());
        }

        foreach (var item in keeperModel.Cars)
        {
            keeperDbContext.Cars.Add(item.ToEf());
        }
        foreach (var item in keeperModel.CarYearMileages)
        {
            keeperDbContext.CarYearMileages.Add(item.ToEf());
        }

        foreach (var item in keeperModel.DepositOffers)
        {
            keeperDbContext.DepositOffers.Add(item.ToEf());
        }
        foreach (var item in keeperModel.DepositConditions)
        {
            keeperDbContext.DepositConditions.Add(item.ToEf());
        }
        foreach (var item in keeperModel.DepositRateLines)
        {
            keeperDbContext.DepositRateLines.Add(item.ToEf());
        }

        await keeperDbContext.SaveChangesAsync();
    }
}
