using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class MoneyBackCategoryEf
{
    public int Id { get; set; }
    public int MoneyBackRulesId { get; set; }

    public int? MCC { get; set; }
    [MaxLength(50)] public string Name { get; set; } = string.Empty;
    [MaxLength(1024)] public string Description { get; set; } = string.Empty;
    public double Percent { get; set; }

    // Навигационное свойство
    public MoneyBackRulesEf MoneyBackRules { get; set; } = null!;
}
