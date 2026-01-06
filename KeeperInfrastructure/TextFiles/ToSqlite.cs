using KeeperDomain;

namespace KeeperInfrastructure;

public class ToSqlite(KeeperDbContext keeperDbContext)
{

    public async Task ConvertFromTextFiles(KeeperModel keeperModel)
    {
       
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
