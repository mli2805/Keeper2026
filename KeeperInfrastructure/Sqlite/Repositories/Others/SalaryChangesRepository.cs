using KeeperDomain;

namespace KeeperInfrastructure;

public class SalaryChangesRepository(KeeperDbContext keeperDbContext)
{
    public List<SalaryChange> GetAllSalaryChanges()
    {
        var result = keeperDbContext.SalaryChanges.Select(sc => sc.FromEf()).ToList();
        return result;
    }
}