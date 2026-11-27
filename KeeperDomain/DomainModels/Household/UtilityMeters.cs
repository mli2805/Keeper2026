namespace KeeperDomain;

public class UtilityMeters
{
    public int Id { get; set; } //PK
    public DateOnly ReadingsDate { get; set; }
    public UtilityMeter ColdWater { get; set; } = new UtilityMeter();
    public UtilityMeter HotWater { get; set; } = new UtilityMeter();
    public UtilityMeter Electricity { get; set; } = new UtilityMeter();
    public UtilityMeter Gas { get; set; } = new UtilityMeter();
}
