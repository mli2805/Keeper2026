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
                existingEf.CbrUsdRate = rate.CbrRate.Usd.Value/rate.CbrRate.Usd.Unit;

                existingEf.UsdRate = rate.NbRates.Usd.Value/rate.NbRates.Usd.Unit;
                existingEf.EuroRate = rate.NbRates.Euro.Value/rate.NbRates.Euro.Unit;
                existingEf.RubRate = rate.NbRates.Rur.Value/rate.NbRates.Rur.Unit;
                existingEf.CnyRate = rate.NbRates.Cny.Value/rate.NbRates.Cny.Unit;
            }
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
