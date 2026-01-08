using KeeperDomain;

namespace KeeperInfrastructure;

public class OfficialRatesRepository(KeeperDbContext keeperDbContext)
{
    public List<OfficialRates> GetAllOfficialRates()
    {
        var result = keeperDbContext.OfficialRates.Select(r=>r.FromEf()).ToList();
        return result; 
    }
}
