using System.Globalization;

namespace KeeperDomain;


[Serializable]
public class OneRate : IDumpable, IParsable<OneRate>
{
    public int Id { get; set; }
    public double Value { get; set; }
    public int Unit { get; set; } = 1;

    public OneRate Clone()
    {
        return (OneRate)MemberwiseClone();
    }

    public string Dump()
    {
        return Value.ToString(CultureInfo.GetCultureInfo("en-US")) + " / " + Unit;
    }

    public OneRate FromString(string s)
    {
        var substrings = s.Split('/');
        Value = double.Parse(substrings[0], CultureInfo.GetCultureInfo("en-US"));
        Unit = int.Parse(substrings[1]);
        return this;
    }
}