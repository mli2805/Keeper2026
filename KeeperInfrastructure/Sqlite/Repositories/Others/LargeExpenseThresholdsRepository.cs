using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class LargeExpenseThresholdsRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<LargeExpenseThreshold> GetAllLargeExpenseThresholds()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.LargeExpenseThresholds.Select(let => let.FromEf()).ToList();
        return result;
    }
}
