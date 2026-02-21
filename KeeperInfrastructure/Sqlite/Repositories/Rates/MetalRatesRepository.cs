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

    public async Task UpdateWholeList(IEnumerable<MetalRate> metalRates)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();

        var uiRates = metalRates.ToList();

        var uiIds = uiRates.Where(r => r.Id != 0)
                           .Select(r => r.Id)
                           .ToHashSet();

        var dbRates = await keeperDbContext.MetalRates.ToListAsync();

        // Удаляем те, которых больше нет в UI
        var toDelete = dbRates.Where(db => !uiIds.Contains(db.Id)).ToList();
        keeperDbContext.MetalRates.RemoveRange(toDelete);

        // Обновляем и добавляем
        foreach (var rate in uiRates)
        {
            if (rate.Id != 0)
            {
                var existing = dbRates.FirstOrDefault(r => r.Id == rate.Id);
                if (existing != null)
                {
                    existing.Date = rate.Date;
                    existing.Metal = rate.Metal;
                    existing.Proba = rate.Proba;
                    existing.Price = rate.Price;
                }
            }
            else
            {
                var newEf = rate.ToEf();
                await keeperDbContext.MetalRates.AddAsync(newEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
    }

}
