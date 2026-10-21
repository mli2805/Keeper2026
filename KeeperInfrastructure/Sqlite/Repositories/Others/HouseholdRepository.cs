using Microsoft.EntityFrameworkCore;
using KeeperModels;

namespace KeeperInfrastructure;

[ExportRepository]
public class HouseholdRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<UtilityMetersModel>> GetAllUtilityMeters()
    {
        await using var context = await factory.CreateDbContextAsync();
        var utilityMetersEf = await context.UtilityMeters.ToListAsync();
        return utilityMetersEf.Select(um => um.ToModel()).ToList();
    }

    public async Task<UtilityMetersModel> AddUtilityMeters(UtilityMetersModel model)
    {
        await using var context = await factory.CreateDbContextAsync();
        var entity = model.ToEf();
        await context.UtilityMeters.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity.ToModel();
    }
}
