using System.Globalization;

namespace KeeperDomain;

public class UtilityMeter : IDumpable
{
    public double From { get; set; }
    public double To { get; set; }
    public double Volume { get; set; }
    public double Tariff { get; set; }

    public string Dump()
    {
        return From + " ; " + To + " ; " + Volume + " ; " + Tariff;
    }

    public UtilityMeter FromString(string from, string to, string volume, string tariff = "0")
    {
        From = double.Parse(from, CultureInfo.InvariantCulture);
        To = double.Parse(to, CultureInfo.InvariantCulture);
        Volume = double.Parse(volume, CultureInfo.InvariantCulture);
        Tariff = double.Parse(tariff, CultureInfo.InvariantCulture);
        return this;
    }
}
