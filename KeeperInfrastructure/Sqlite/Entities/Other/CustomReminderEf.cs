using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class CustomReminderEf
{
    public int Id { get; set; }
    public bool Enabled { get; set; }
    public DateTime TriggerDate { get; set; }

    [MaxLength(32)] public string Every { get; set; } = string.Empty;

    [MaxLength(1000)] public string Memo { get; set; } = string.Empty;
}
