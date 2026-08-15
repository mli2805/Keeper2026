using KeeperDomain;
using KeeperModels;

namespace KeeperInfrastructure;

public static class TodoTaskMapper
{
    public static TodoTaskEf ToEf(this TodoTask task)
    {
        return new TodoTaskEf
        {
            Id = task.Id,
            Title = task.Title,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            Importance = task.Importance
        };
    }

    public static TodoSubtaskEf ToEf(this TodoSubtask subtask)
    {
        return new TodoSubtaskEf
        {
            Id = subtask.Id,
            TodoTaskId = subtask.TodoTaskId,
            Ordinal = subtask.Ordinal,
            Title = subtask.Title,
            IsCompleted = subtask.IsCompleted
        };
    }

    public static TodoTaskModel ToModel(this TodoTaskEf taskEf)
    {
        return new TodoTaskModel
        {
            Id = taskEf.Id,
            Title = taskEf.Title,
            CreatedAt = taskEf.CreatedAt,
            CompletedAt = taskEf.CompletedAt,
            Importance = taskEf.Importance,
            IsCompleted = taskEf.CompletedAt != null,
            Subtasks = taskEf.Subtasks.OrderBy(s => s.Ordinal).ThenBy(s => s.Id).Select(s => s.ToModel()).ToList()
        };
    }

    public static TodoSubtaskModel ToModel(this TodoSubtaskEf subtaskEf)
    {
        return new TodoSubtaskModel
        {
            Id = subtaskEf.Id,
            TodoTaskId = subtaskEf.TodoTaskId,
            Ordinal = subtaskEf.Ordinal,
            Title = subtaskEf.Title,
            IsCompleted = subtaskEf.IsCompleted
        };
    }

    public static TodoTaskEf ToEf(this TodoTaskModel taskModel)
    {
        taskModel.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        return new TodoTaskEf
        {
            Id = taskModel.Id,
            Title = taskModel.Title,
            CreatedAt = taskModel.CreatedAt,
            CompletedAt = taskModel.CompletedAt,
            Importance = taskModel.Importance,
            Subtasks = taskModel.Subtasks.Select(s => s.ToEf()).ToList()
        };
    }

    public static TodoSubtaskEf ToEf(this TodoSubtaskModel subtaskModel)
    {
        return new TodoSubtaskEf
        {
            Id = subtaskModel.Id,
            TodoTaskId = subtaskModel.TodoTaskId,
            Ordinal = subtaskModel.Ordinal,
            Title = subtaskModel.Title,
            IsCompleted = subtaskModel.IsCompleted
        };
    }

    public static TodoTask FromModel(this TodoTaskModel model)
    {
        model.NormalizeCompletion(DateOnly.FromDateTime(DateTime.Now));

        return new TodoTask
        {
            Id = model.Id,
            Title = model.Title,
            CreatedAt = model.CreatedAt,
            CompletedAt = model.CompletedAt,
            Importance = model.Importance
        };
    }

    public static TodoSubtask FromModel(this TodoSubtaskModel model)
    {
        return new TodoSubtask
        {
            Id = model.Id,
            TodoTaskId = model.TodoTaskId,
            Ordinal = model.Ordinal,
            Title = model.Title,
            IsCompleted = model.IsCompleted
        };
    }
}
