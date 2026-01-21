using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class MetalRatesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<MetalRate> GetAllMetalRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.MetalRates.Select(r => r.FromEf()).ToList();
        return result;
    }

    public async Task SaveMetalRates(IEnumerable<MetalRate> metalRates)
    {
        using var keeperDbContext = factory.CreateDbContext();

        // таблица маленькая и нет операции удаления записей

        var ids = metalRates.Where(r => r.Id != 0).Select(r => r.Id).ToList();
        var existingRates = keeperDbContext.MetalRates.Where(r => ids.Contains(r.Id)).ToDictionary(r => r.Id);

        foreach (var rate in metalRates)
        {
            if (rate.Id != 0 && existingRates.ContainsKey(rate.Id))
            {
                // обновляем существующую запись
                MetalRateEf rateEf = existingRates[rate.Id];
                rateEf.Date = rate.Date;
                rateEf.Metal = rate.Metal;
                rateEf.Proba = rate.Proba;
                rateEf.Price = rate.Price;
            }
            else
            {
                // добавляем новую запись
                var rateEf = rate.ToEf();
                await keeperDbContext.MetalRates.AddAsync(rateEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
    }
}
