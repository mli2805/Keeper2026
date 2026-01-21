using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class RefinancingRatesRepository(IDbContextFactory<KeeperDbContext> factory) 
{
    public List<RefinancingRate> GetAllRefinancingRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.RefinancingRates.Select(r=>r.FromEf()).ToList();
        return result;
    }
}
