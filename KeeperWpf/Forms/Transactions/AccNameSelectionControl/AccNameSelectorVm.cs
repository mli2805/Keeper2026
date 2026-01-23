using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;

namespace KeeperWpf;

public class AccNameSelectorVm : PropertyChangedBase
{
    public List<AccNameButtonVm> Buttons { get; set; } = null!;

    private AccName _myAccName = null!;
    public AccName MyAccName
    {
        get { return _myAccName; }
        set
        {
            if (Equals(value, _myAccName)) return;
            _myAccName = value;
            NotifyOfPropertyChange();
        }
    }

    private List<AccName> _availableAccNames = null!;
    public List<AccName> AvailableAccNames
    {
        get { return _availableAccNames; }
        set
        {
            if (Equals(value, _availableAccNames)) return;
            _availableAccNames = value;
            NotifyOfPropertyChange();
        }
    }

    public string ControlTitle { get; set; } = string.Empty;

    public Visibility Visibility { get; set; }

}
