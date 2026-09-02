using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class OfficialRatesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<OfficialRates> GetAllOfficialRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.OfficialRates.Select(r=>r.FromEf()).ToList();
        return result; 
    }

    public async Task Add(List<OfficialRates> rates)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var ratesEf = rates.Select(r=>r.ToEf());
        keeperDbContext.OfficialRates.AddRange(ratesEf);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task UpdateSome(List<OfficialRates> rates)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        foreach (var rate in rates)
        {
            var existingEf = keeperDbContext.OfficialRates.FirstOrDefault(r => r.Id == rate.Id);
            if (existingEf != null)
            {
                existingEf.CbrUsdRate = rate.CbrRate.Usd.Value;

                existingEf.UsdRate = rate.NbRates.Usd.Value;
                existingEf.EuroRate = rate.NbRates.Euro.Value;
                existingEf.RubRate = rate.NbRates.Rur.Value;
                existingEf.CnyRate = rate.NbRates.Cny.Value;
            }
        }
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task Fix()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var from = new DateTime(2026, 2, 18);
        var rates = await keeperDbContext.OfficialRates
            .Where(r => r.Date >= from && (r.RubRate > 1 || r.CnyRate > 1))
            .ToListAsync();

        foreach (var rate in rates)
        {
            if (rate.RubRate > 1)
                rate.RubRate /= 100;

            if (rate.CnyRate > 1)
                rate.CnyRate /= 10;
        }

        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteRate(int rateId)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var rateEf = await keeperDbContext.OfficialRates.FirstOrDefaultAsync(r => r.Id == rateId);
        if (rateEf != null)
        {
            keeperDbContext.OfficialRates.Remove(rateEf);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
