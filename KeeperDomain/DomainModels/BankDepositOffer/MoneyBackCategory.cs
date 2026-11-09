namespace KeeperDomain;

// может быть как для начисления манибэка, так и для исключения из более широкой категории
using System.Globalization;

public class MoneyBackCategory : IDumpable, IParsable<MoneyBackCategory>
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public int? MCC { get; set; } // не всегда указывают, но иногда пишут MCC категории
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public string Dump()
    {
        return Id + " ; " + MoneyBackRulesId + " ; " + 
            MCC + " ; " + Name + " ; " + Description + " ; " +
            Percent.ToString(new CultureInfo("en-US"));
    }

    public MoneyBackCategory FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        MoneyBackRulesId = int.Parse(substrings[1]);
        MCC = int.TryParse(substrings[2], out var mcc) ? mcc : null;
        Name = substrings[3].Trim();
        Description = substrings[4].Trim();
        Percent = double.Parse(substrings[5], CultureInfo.InvariantCulture);

        return this;
    }
}
