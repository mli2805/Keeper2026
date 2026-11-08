using System.Globalization;

namespace KeeperDomain;

public class MoneyBackPartner : IDumpable, IParsable<MoneyBackPartner>
{
    public int Id { get; set; } //PK
    public int MoneyBackRulesId { get; set; }

    public string Name { get; set; } = string.Empty;
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public string Dump()
    {
        return Id + " ; " + MoneyBackRulesId + " ; " +  Name + " ; " + 
            Percent.ToString(new CultureInfo("en-US"));
    }

    public MoneyBackPartner FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        MoneyBackRulesId = int.Parse(substrings[1]);
        Name = substrings[2].Trim();
        Percent = double.Parse(substrings[3], CultureInfo.InvariantCulture);

        return this;
    }
}
