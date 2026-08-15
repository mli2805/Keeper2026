using KeeperDomain;
using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class TodoTaskEf
{
    public int Id { get; set; }
    [MaxLength(1000)] public string Title { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
    public DateOnly? CompletedAt { get; set; }
    public TodoImportance Importance { get; set; } = TodoImportance.Normal;

    public List<TodoSubtaskEf> Subtasks { get; set; } = [];
}
