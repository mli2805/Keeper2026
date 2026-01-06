namespace KeeperInfrastructure;

// курсы обмена валют (покупка-продажа)
public class ExchangeRatesEf
{
    public int Id { get; set; } //PK
    public DateTime Date { get; set; }

    // на табло 2.5 - 2.6
    public double BynUsdA { get; set; } // 2.5
    public double BynUsdB { get; set; } // 2.6

    public double BynEurA { get; set; }
    public double BynEurB { get; set; }

    // хранить не за 100, а за 1 (для унификации)
    public double BynRubA { get; set; }
    public double BynRubB { get; set; }

    // 1.05 - 1.10
    public double EurUsdA { get; set; } // 1.05
    public double EurUsdB { get; set; } // 1.10

    // 56 - 72.5
    public double RubUsdA { get; set; } // 56
    public double RubUsdB { get; set; } // 72.5

    public double RubEurA { get; set; }
    public double RubEurB { get; set; }
}
