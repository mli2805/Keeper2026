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
}
