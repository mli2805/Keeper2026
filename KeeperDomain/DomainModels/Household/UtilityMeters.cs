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
        ReadingsDate = DateOnly.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
        var valuesPerMeter = substrings.Length >= 18 ? 4 : 3;
        ColdWater = ParseMeter(2);
        HotWater = ParseMeter(2 + valuesPerMeter);
        Electricity = ParseMeter(2 + 2 * valuesPerMeter);
        Gas = ParseMeter(2 + 3 * valuesPerMeter);
        return this;

        UtilityMeter ParseMeter(int index) => new UtilityMeter().FromString(
            substrings[index].Trim(),
            substrings[index + 1].Trim(),
            substrings[index + 2].Trim(),
            valuesPerMeter == 4 ? substrings[index + 3].Trim() : "0");
    }
}
