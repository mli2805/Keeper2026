using System.Globalization;

namespace KeeperDomain;

[Serializable]
public class Fuelling : IDumpable, IParsable<Fuelling>
{
    private static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
  
    public int Id { get; set; } //PK
    public int TransactionId { get; set; }

    public int CarAccountId { get; set; }
    public double Volume { get; set; }
    public FuelType FuelType { get; set; }

    public string Dump()
    {
        return Id + " ; " + TransactionId + " ; " + CarAccountId + " ; " +
               Volume + " ; " + FuelType;
    }

    public Fuelling FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0].Trim());
        TransactionId = int.Parse(substrings[1].Trim());
        CarAccountId = int.Parse(substrings[2].Trim());
        Volume = double.Parse(substrings[3].Trim(), _enUsCulture);
        FuelType = (FuelType)Enum.Parse(typeof(FuelType), substrings[4]);
        return this;
    }
}