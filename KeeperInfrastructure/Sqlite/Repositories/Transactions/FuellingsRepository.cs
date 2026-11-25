using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class FuellingsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<FuellingModel> GetAllFuellingModels(KeeperDataModel dataModel)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = new List<FuellingModel>();
        var fuellings = keeperDbContext.Fuellings.Select(f => f.FromEf()).ToList();
        foreach (var fuelling in fuellings)
        {
            var tr = dataModel.Transactions[fuelling.TransactionId];

            var fuellingModel = new FuellingModel()
            {
                Id = fuelling.Id,
                Timestamp = tr.Timestamp,
                Transaction = tr,
                CarAccountId = fuelling.CarAccountId,
                Amount = tr.Amount,
                Currency = tr.Currency,
                Volume = fuelling.Volume,
                FuelType = fuelling.FuelType,
                Comment = tr.Comment,

            };

            var oneLitrePrice = Math.Abs(fuelling.Volume) < 0.01 ? 0 : tr.Amount / (decimal)fuelling.Volume;
            switch (tr.Currency)
            {
                case CurrencyCode.BYR:
                case CurrencyCode.BYN:
                    {
                        fuellingModel.OneLitrePrice = oneLitrePrice;
                        fuellingModel.OneLitreInUsd = dataModel.AmountInUsd(tr.Timestamp, tr.Currency, oneLitrePrice);
                        break;
                    }
                case CurrencyCode.EUR:
                    {
                        var rate = dataModel.GetRate(tr.Timestamp, tr.Currency);
                        fuellingModel.OneLitrePrice = oneLitrePrice * (decimal)rate!.Value;
                        var rateUsd = dataModel.GetRate(tr.Timestamp, CurrencyCode.EUR, true);
                        fuellingModel.OneLitreInUsd = oneLitrePrice * (decimal)rateUsd!.Value;
                        break;
                    }
                case CurrencyCode.USD:
                    {
                        var rate = dataModel.GetRate(tr.Timestamp, tr.Timestamp < new DateTime(2016, 7, 1) ? CurrencyCode.BYR : CurrencyCode.BYN);
                        fuellingModel.OneLitrePrice = oneLitrePrice * (decimal)rate!.Value;
                        fuellingModel.OneLitreInUsd = oneLitrePrice;
                        break;
                    }
            }
            result.Add(fuellingModel);
        }

        return result;
    }

    public async Task AddFuelling(FuellingModel fuellingModel)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var fuellingEf = fuellingModel.ToEf();
        await keeperDbContext.Fuellings.AddAsync(fuellingEf);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteFuelling(int fuellingId)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var fuellingEf = await keeperDbContext.Fuellings.FirstOrDefaultAsync(f => f.Id == fuellingId);
        if (fuellingEf != null)
        {
            keeperDbContext.Fuellings.Remove(fuellingEf);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
