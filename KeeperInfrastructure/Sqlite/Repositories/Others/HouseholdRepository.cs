using Microsoft.EntityFrameworkCore;
using KeeperDomain;

namespace KeeperInfrastructure;

[ExportRepository]
public class HouseholdRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<UtilityMeters>> GetAllUtilityMeters()
    {
        await using var context = await factory.CreateDbContextAsync();
        var utilityMetersEf = await context.UtilityMeters.ToListAsync();
        return utilityMetersEf.Select(um => um.FromEf()).ToList();
    }
}
