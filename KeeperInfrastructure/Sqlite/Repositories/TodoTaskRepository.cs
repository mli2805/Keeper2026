using KeeperModels;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

[ExportRepository]
public class TodoTaskRepository(IDbContextFactory<KeeperDbContext> factory)
{
    public async Task<List<TodoTaskModel>> GetAllTodoTasksWithSubtasks()
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var tasks = await keeperDbContext.TodoTasks
            .Include(t => t.Subtasks)
            .ToListAsync();

        return tasks.Select(t => t.ToModel()).ToList();
    }

    public async Task<TodoTaskModel> Add(TodoTaskModel model)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        if (model.CreatedAt == default)
        {
            model.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
        }
        NormalizeSubtaskOrdinals(model);
        model.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        var entity = model.ToEf();
        await keeperDbContext.TodoTasks.AddAsync(entity);
        await keeperDbContext.SaveChangesAsync();
        return entity.ToModel();
    }

    public async Task<TodoTaskModel> Update(TodoTaskModel model)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entity = await keeperDbContext.TodoTasks
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == model.Id);

        NormalizeSubtaskOrdinals(model);
        model.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        if (entity == null)
        {
            return await Add(model);
        }

        entity.Title = model.Title;
        entity.CreatedAt = model.CreatedAt == default ? entity.CreatedAt : model.CreatedAt;
        entity.CompletedAt = model.CompletedAt;
        entity.Importance = model.Importance;

        foreach (var subtask in model.Subtasks)
        {
            var subtaskEf = entity.Subtasks.FirstOrDefault(s => s.Id == subtask.Id);
            if (subtaskEf == null)
            {
                subtaskEf = subtask.ToEf();
                subtaskEf.TodoTaskId = entity.Id;
                await keeperDbContext.TodoSubtasks.AddAsync(subtaskEf);
            }
            else
            {
                subtaskEf.Ordinal = subtask.Ordinal;
                subtaskEf.Title = subtask.Title;
                subtaskEf.IsCompleted = subtask.IsCompleted;
            }
        }

        foreach (var subtaskEf in entity.Subtasks.ToList())
        {
            if (model.Subtasks.FirstOrDefault(s => s.Id == subtaskEf.Id) == null)
            {
                keeperDbContext.TodoSubtasks.Remove(subtaskEf);
            }
        }

        await keeperDbContext.SaveChangesAsync();
        return entity.ToModel();
    }

    private static void NormalizeSubtaskOrdinals(TodoTaskModel model)
    {
        for (int i = 0; i < model.Subtasks.Count; i++)
        {
            model.Subtasks[i].Ordinal = i;
        }
    }

    public async Task Delete(int id)
    {
        await using var keeperDbContext = await factory.CreateDbContextAsync();
        var entity = await keeperDbContext.TodoTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (entity != null)
        {
            keeperDbContext.TodoTasks.Remove(entity);
            await keeperDbContext.SaveChangesAsync();
        }
    }
}
