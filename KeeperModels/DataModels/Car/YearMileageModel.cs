using Caliburn.Micro;
using KeeperDomain;

namespace KeeperModels;

public class YearMileageModel : PropertyChangedBase
{
    public int Id { get; set; } //PK
    public int CarId { get; set; }

    private int _odometer;
    public int Odometer
    {
        get => _odometer;
        set { _odometer = value; NotifyOfPropertyChange(); }
    }

    //вычисляемые поля
    private int _yearNumber;
    public int YearNumber
    {
        get => _yearNumber;
        set { _yearNumber = value; NotifyOfPropertyChange(); }
    }

    private Period _period = null!;
    public Period Period
    {
        get => _period;
        set { _period = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(FromTo)); }
    }

    public string FromTo => Period.ToStringD();

    private int _mileage;
    public int Mileage
    {
        get => _mileage;
        set { _mileage = value; NotifyOfPropertyChange(); }
    }

    private decimal _yearAmount;
    public decimal YearAmount
    {
        get => _yearAmount;
        set { _yearAmount = value; NotifyOfPropertyChange(); }
    }

    private decimal _dayAmount;
    public decimal DayAmount
    {
        get => _dayAmount;
        set { _dayAmount = value; NotifyOfPropertyChange(); }
    }

    public decimal PricePerKm => Mileage > 0 ? YearAmount / Mileage : 0;
}