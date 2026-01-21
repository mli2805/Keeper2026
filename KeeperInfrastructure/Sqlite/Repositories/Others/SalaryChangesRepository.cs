using KeeperDomain;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class SalaryChangesRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public List<SalaryChange> GetAllSalaryChanges()
    {
        using var keeperDbContext = factory.CreateDbContext();
        var result = keeperDbContext.SalaryChanges.Select(sc => sc.FromEf()).ToList();
        return result;
    }
}