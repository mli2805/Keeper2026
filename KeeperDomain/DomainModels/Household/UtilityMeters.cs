using System.Globalization;

namespace KeeperDomain;

public class UtilityMeters : IDumpable, IParsable<UtilityMeters>
{
    public int Id { get; set; } //PK
    public DateOnly ReadingsDate { get; set; }
    public UtilityMeter ColdWater { get; set; } = new UtilityMeter();
    public UtilityMeter HotWater { get; set; } = new UtilityMeter();
    public UtilityMeter Electricity { get; set; } = new UtilityMeter();
    public UtilityMeter Gas { get; set; } = new UtilityMeter();

    public string Dump()
    {
        return Id + " ; " + ReadingsDate.ToString("dd/MM/yyyy") + " ; " +
               ColdWater.Dump() + " ; " + HotWater.Dump() + " ; " +
               Electricity.Dump() + " ; " + Gas.Dump();
    }

    public UtilityMeters FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        ReadingsDate = DateOnly.ParseExact(substrings[1].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        ColdWater = new UtilityMeter().FromString(substrings[2].Trim(), substrings[3].Trim(), substrings[4].Trim());
        HotWater = new UtilityMeter().FromString(substrings[5].Trim(), substrings[6].Trim(), substrings[7].Trim());
        Electricity = new UtilityMeter().FromString(substrings[8].Trim(), substrings[9].Trim(), substrings[10].Trim());
        Gas = new UtilityMeter().FromString(substrings[11].Trim(), substrings[12].Trim(), substrings[13].Trim());
        return this;
    }
}
