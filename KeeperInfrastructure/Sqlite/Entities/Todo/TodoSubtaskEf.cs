using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class TodoSubtaskEf
{
    public int Id { get; set; }
    public int TodoTaskId { get; set; }
    public int Ordinal { get; set; }
    [MaxLength(1000)] public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    public TodoTaskEf TodoTask { get; set; } = null!;
}
