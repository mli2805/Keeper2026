using KeeperDomain;

namespace KeeperInfrastructure;

public class ExchangeRatesRepository(KeeperDbContext keeperDbContext)
{
    public List<ExchangeRates> GetAllExchangeRates()
    {
        var result = keeperDbContext.ExchangeRates.Select(r=>r.FromEf()).ToList();
        return result;
    }
}
