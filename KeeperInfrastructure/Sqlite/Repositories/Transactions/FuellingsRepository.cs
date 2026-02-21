using KeeperDomain;
using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class FuellingsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<Fuelling> GetAllFuellings()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.Fuellings.Select(f => f.FromEf()).ToList();
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
