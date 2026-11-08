namespace KeeperInfrastructure;

public class MoneyBackPartnerEf
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public string Name { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа

    // Навигационное свойство
    public MoneyBackRulesEf MoneyBackRules { get; set; } = null!;
}
