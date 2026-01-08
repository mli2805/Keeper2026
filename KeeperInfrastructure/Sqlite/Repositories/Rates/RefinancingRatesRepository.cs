using KeeperDomain;

namespace KeeperInfrastructure;

public class RefinancingRatesRepository(KeeperDbContext keeperDbContext) 
{
    public List<RefinancingRate> GetAllRefinancingRates()
    {
        var result = keeperDbContext.RefinancingRates.Select(r=>r.FromEf()).ToList();
        return result;
    }
}
