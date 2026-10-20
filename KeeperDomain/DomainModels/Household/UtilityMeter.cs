using System.Globalization;

namespace KeeperDomain;

public class UtilityMeter : IDumpable
{
    public double From { get; set; }
    public double To { get; set; }
    public double Volume { get; set; }

    public string Dump()
    {
        return From + " ; " + To + " ; " + Volume;
    }

    public UtilityMeter FromString(string from, string to, string volume)
    {
        From = double.Parse(from, CultureInfo.InvariantCulture);
        To = double.Parse(to, CultureInfo.InvariantCulture);
        Volume = double.Parse(volume, CultureInfo.InvariantCulture);
        return this;
    }
}
