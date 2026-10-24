
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

    private decimal? _electricityPaymentAmount;
    public decimal? ElectricityPaymentAmount
    {
        get => _electricityPaymentAmount;
        set
        {
            if (_electricityPaymentAmount == value) return;
            _electricityPaymentAmount = value;
            NotifyOfPropertyChange(() => ElectricityPaymentAmount);
        }
    }

    private string _electricityComment = string.Empty;
    public string ElectricityComment
    {
        get => _electricityComment;
        set
        {
            if (_electricityComment == value) return;
            _electricityComment = value;
            NotifyOfPropertyChange(() => ElectricityComment);
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

    private decimal? _gasPaymentAmount;
    public decimal? GasPaymentAmount
    {
        get => _gasPaymentAmount;
        set
        {
            if (_gasPaymentAmount == value) return;
            _gasPaymentAmount = value;
            NotifyOfPropertyChange(() => GasPaymentAmount);
        }
    }

    private string _gasComment = string.Empty;
    public string GasComment
    {
        get => _gasComment;
        set
        {
            if (_gasComment == value) return;
            _gasComment = value;
            NotifyOfPropertyChange(() => GasComment);
        }
    }
}
