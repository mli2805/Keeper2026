using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class TodoTask
{
    public int Id { get; set; }
    [MaxLength(1000)] public string Title { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
    public DateOnly? CompletedAt { get; set; }
    public TodoImportance Importance { get; set; } = TodoImportance.Normal;
    public TodoCategory Category { get; set; } = TodoCategory.CountryHouse;
}
