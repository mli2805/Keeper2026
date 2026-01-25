using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

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
        using var keeperDbContext = factory.CreateDbContext();
        var ratesEf = rates.Select(r=>r.ToEf());
        keeperDbContext.OfficialRates.AddRange(ratesEf);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task UpdateSome(List<OfficialRates> rates)
    {
        using var keeperDbContext = factory.CreateDbContext();
        foreach (var rate in rates)
        {
            var existingEf = keeperDbContext.OfficialRates.FirstOrDefault(r => r.Id == rate.Id);
            if (existingEf != null)
            {
                var existing = existingEf.FromEf();
                existing.NbRates = rate.NbRates;
                existing.CbrRate = rate.CbrRate;
                existingEf = existing.ToEf();
            }
        }
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteRate(int rateId)
    {
        using var keeperDbContext = factory.CreateDbContext();
        var rateEf = await keeperDbContext.OfficialRates.FirstOrDefaultAsync(r => r.Id == rateId);
        if (rateEf != null)
        {
            keeperDbContext.OfficialRates.Remove(rateEf);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
