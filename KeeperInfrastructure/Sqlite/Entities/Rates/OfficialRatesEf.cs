namespace KeeperInfrastructure;

public class OfficialRatesEf
{
    public int Id { get; set; } //PK
    public DateTime Date { get; set; }

    // храним курсы НБРБ за 1 единицу иностранной валюты
    // показываем c 1.07.2016 за 100 rub, за 10 cny
    public double UsdRate { get; set; }
    public double EuroRate { get; set; }
    public double RubRate { get; set; }
    public double CnyRate { get; set; }

    // курс ЦБРФ за 1 единицу иностранной валюты
    public double CbrUsdRate { get; set; }

}
