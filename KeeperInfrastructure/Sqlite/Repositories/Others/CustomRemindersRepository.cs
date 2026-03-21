using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class CustomRemindersRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<CustomReminderModel>> GetAllCustomReminders()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var result = keeperDbContext.CustomReminders.Select(cr => cr.ToModel()).ToList();
        return result;
    }

    public async Task SaveAll(List<CustomReminderModel> models)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entities = models.Select(m => m.ToEf()).ToList();
        keeperDbContext.CustomReminders.RemoveRange(keeperDbContext.CustomReminders);
        await keeperDbContext.CustomReminders.AddRangeAsync(entities);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task<CustomReminderModel> Add(CustomReminderModel model)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entity = model.ToEf();
        await keeperDbContext.CustomReminders.AddAsync(entity);
        await keeperDbContext.SaveChangesAsync();
        return entity.ToModel();
    }

    public async Task<CustomReminderModel> Update(CustomReminderModel model)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entity = model.ToEf();
        keeperDbContext.CustomReminders.Update(entity);
        await keeperDbContext.SaveChangesAsync();
        return entity.ToModel();
    }

    public async Task Delete(int id)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entity = await keeperDbContext.CustomReminders.FirstOrDefaultAsync(cr => cr.Id == id);
        if (entity != null)
        {
            keeperDbContext.CustomReminders.Remove(entity);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
