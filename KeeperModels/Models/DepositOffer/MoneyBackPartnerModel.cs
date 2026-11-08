namespace KeeperModels;

public class MoneyBackPartnerModel
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public string Name { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа
}
