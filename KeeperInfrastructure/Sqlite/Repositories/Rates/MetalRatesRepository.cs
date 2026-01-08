using KeeperDomain;

namespace KeeperInfrastructure;

public class MetalRatesRepository(KeeperDbContext keeperDbContext)
{
    public List<MetalRate> GetAllMetalRates()
    {
        var result = keeperDbContext.MetalRates.Select(r=>r.FromEf()).ToList();
        return result;
    }
}
