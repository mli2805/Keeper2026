
using Caliburn.Micro;

namespace KeeperModels;

public class UtilityMetersModel : PropertyChangedBase
{
    public int Id { get; set; } //PK

    private DateTime _readingsDate;
    public DateTime ReadingsDate
    {
        get => _readingsDate;
        set
        {
            if (_readingsDate != value)
            {
                _readingsDate = value;
                NotifyOfPropertyChange(() => ReadingsDate);
            }
        }
    }

    private UtilityMeterModel _coldWater = new();
    public UtilityMeterModel ColdWater
    {
        get => _coldWater;
        set
        {
            if (_coldWater != value)
            {
                _coldWater = value;
                NotifyOfPropertyChange(() => ColdWater);
            }
        }
    }

    private UtilityMeterModel _hotWater = new();
    public UtilityMeterModel HotWater
    {
        get => _hotWater;
        set
        {
            if (_hotWater != value)
            {
                _hotWater = value;
                NotifyOfPropertyChange(() => HotWater);
            }
        }
    }

    private UtilityMeterModel _electricity = new();
    public UtilityMeterModel Electricity
    {
        get => _electricity;
        set
        {
            if (_electricity != value)
            {
                _electricity = value;
                NotifyOfPropertyChange(() => Electricity);
            }
        }
    }

    private UtilityMeterModel _gas = new();
    public UtilityMeterModel Gas
    {
        get => _gas;
        set
        {
            if (_gas != value)
            {
                _gas = value;
                NotifyOfPropertyChange(() => Gas);
            }
        }
    }
}
