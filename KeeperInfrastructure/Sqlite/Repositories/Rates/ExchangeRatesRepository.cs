using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class ExchangeRatesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<ExchangeRates> GetAllExchangeRates()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.ExchangeRates.Select(r=>r.FromEf()).ToList();
        return result;
    }
}
