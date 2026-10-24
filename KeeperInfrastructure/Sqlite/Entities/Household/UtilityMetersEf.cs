namespace KeeperInfrastructure;

public class UtilityMetersEf
{
    public int Id { get; set; } //PK
    public DateOnly ReadingsDate { get; set; }

    public double ColdWaterFrom { get; set; }
    public double ColdWaterTo { get; set; }
    public double ColdWaterVolume { get; set; }
    public double ColdWaterTariff { get; set; }

    public double HotWaterFrom { get; set; }
    public double HotWaterTo { get; set; }
    public double HotWaterVolume { get; set; }
    public double HotWaterTariff { get; set; }

    public double ElectricityFrom { get; set; }
    public double ElectricityTo { get; set; }
    public double ElectricityVolume { get; set; }
    public double ElectricityTariff { get; set; }

    public double GasFrom { get; set; }
    public double GasTo { get; set; }
    public double GasVolume { get; set; }
    public double GasTariff { get; set; }

}
