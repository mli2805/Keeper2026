using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class RefinancingRatesRepository(IDbContextFactory<KeeperDbContext> factory) 
{
    public List<RefinancingRate> GetAll()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.RefinancingRates.Select(r=>r.FromEf()).ToList();
        return result;
    }

    public void Add(RefinancingRate refinancingRate)
    {
        using var keeperDbContext = factory.CreateDbContext();
        keeperDbContext.RefinancingRates.Add(refinancingRate.ToEf());
        keeperDbContext.SaveChanges();
    }
}
