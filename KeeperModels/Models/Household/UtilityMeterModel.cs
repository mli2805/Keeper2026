using Caliburn.Micro;

namespace KeeperModels;

public class UtilityMeterModel : PropertyChangedBase
{
    private double _from;
    public double From
    {
        get => _from;
        set
        {
            if (_from != value)
            {
                _from = value;
                NotifyOfPropertyChange(() => From);
            }
        }
    }

    private double _to;
    public double To
    {
        get => _to;
        set
        {
            if (_to != value)
            {
                _to = value;
                NotifyOfPropertyChange(() => To);
            }
        }
    }

    private double _volume;
    public double Volume
    {
        get => _volume;
        set
        {
            if (_volume != value)
            {
                _volume = value;
                NotifyOfPropertyChange(() => Volume);
            }
        }
    }
}
