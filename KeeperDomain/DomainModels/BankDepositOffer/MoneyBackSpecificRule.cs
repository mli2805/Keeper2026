namespace KeeperDomain;

// может быть как SpecificCategory, так и PartnerCounterparty
using System.Globalization;

public class MoneyBackSpecificRule : IDumpable, IParsable<MoneyBackSpecificRule>
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public int MCC { get; set; } // не всегда указывают, но иногда пишут MCC категории
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public string Dump()
    {
        return Id + " ; " + MoneyBackRulesId + " ; " + 
             MCC + " ; " + Name + " ; " + Description + " ; " +
            Percent.ToString(new CultureInfo("en-US"));
    }

    public MoneyBackSpecificRule FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        MoneyBackRulesId = int.Parse(substrings[1]);
        MCC = int.Parse(substrings[2]);
        Name = substrings[3].Trim();
        Description = substrings[4].Trim();
        Percent = double.Parse(substrings[5], CultureInfo.InvariantCulture);

        return this;
    }
}
