namespace KeeperModels;

public class TodoSubtaskModel
{
    public int Id { get; set; }
    public int TodoTaskId { get; set; }
    public int Ordinal { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
