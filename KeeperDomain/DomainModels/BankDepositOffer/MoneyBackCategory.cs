namespace KeeperDomain;

// может быть как для начисления манибэка, так и для исключения из более широкой категории

public class MoneyBackCategory
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public int? MCC { get; set; } // не всегда указывают, но иногда пишут MCC категории
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа
}
