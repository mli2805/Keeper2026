using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class ExchangeRatesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<ExchangeRates> GetAllExchangeRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.ExchangeRates.Select(r => r.FromEf()).ToList();
        return result;
    }

    public async Task Add(List<ExchangeRates> rates)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var ratesEf = rates.Select(r => r.ToEf());
        keeperDbContext.ExchangeRates.AddRange(ratesEf);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task Delete(DateTime date)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var rate = await keeperDbContext.ExchangeRates.FirstOrDefaultAsync(r => r.Date == date);
        if (rate != null)
        {
            keeperDbContext.ExchangeRates.Remove(rate);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
