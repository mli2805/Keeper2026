using KeeperDomain;

namespace KeeperInfrastructure;

public class LargeExpenseThresholdsRepository(KeeperDbContext keeperDbContext)
{
    public List<LargeExpenseThreshold> GetAllLargeExpenseThresholds()
    {
        var result = keeperDbContext.LargeExpenseThresholds.Select(let => let.FromEf()).ToList();
        return result;
    }
}
