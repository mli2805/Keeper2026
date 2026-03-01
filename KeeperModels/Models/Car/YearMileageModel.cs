using KeeperDomain;

namespace KeeperModels;

public class YearMileageModel
{
    public int Id { get; set; } //PK
    public int CarId { get; set; }
    public int Odometer { get; set; }

    //вычисляемые поля
    public int YearNumber { get; set; } // ordinal
    public Period Period { get; set; } = null!;
    public string FromTo => Period.ToStringD();
    public int Mileage { get; set; }
    public decimal YearAmount { get; set; }
    public decimal DayAmount { get; set; }

}