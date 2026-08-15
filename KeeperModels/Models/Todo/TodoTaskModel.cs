using KeeperDomain;

namespace KeeperModels;

public class TodoTaskModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
    public DateOnly? CompletedAt { get; set; }
    public TodoImportance Importance { get; set; } = TodoImportance.Normal;
    public List<TodoSubtaskModel> Subtasks { get; set; } = [];

    public bool IsCompleted { get; set; }
    public int SubtasksCount => Subtasks.Count;
    public int CompletedSubtasksCount => Subtasks.Count(s => s.IsCompleted);
    public string ProgressText => $"{CompletedSubtasksCount}/{SubtasksCount}";
    public string SearchText => string.Join(' ', new[] { Title }.Concat(Subtasks.Select(s => s.Title)));

    public void NormalizeCompletion(DateOnly today)
    {
        if (Subtasks.Count > 0)
        {
            IsCompleted = Subtasks.All(s => s.IsCompleted);
        }

        if (IsCompleted)
        {
            CompletedAt ??= today;
        }
        else
        {
            CompletedAt = null;
        }
    }
}
