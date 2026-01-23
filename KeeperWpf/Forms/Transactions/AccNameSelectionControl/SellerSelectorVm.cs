using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace KeeperWpf;

public class SellerSelectorVm : PropertyChangedBase
{
    public List<AccNameButtonVm> Buttons { get; set; } = null!;

    private AccName _myAccName = null!;
    public AccName MyAccName
    {
        get => _myAccName;
        set
        {
            if (Equals(value, _myAccName)) return;
            _myAccName = value;
            NotifyOfPropertyChange();

            SelectedShop = Shops.FindThroughTheForestById(value.Id) ?? Shops.First();
            SelectedMed = Meds.FindThroughTheForestById(value.Id) ?? Meds.First();
        }
    }

    private List<AccName> _availableAccNames = null!;

    public List<AccName> AvailableAccNames
    {
        get => _availableAccNames;
        set
        {
            if (Equals(value, _availableAccNames)) return;
            _availableAccNames = value;
            NotifyOfPropertyChange();
        }
    }

    public List<AccName> Shops { get; set; } = null!;

    private AccName _selectedShop = null!;
    public AccName SelectedShop
    {
        get => _selectedShop;
        set
        {
            if (Equals(value, _selectedShop)) return;
            _selectedShop = value;
            NotifyOfPropertyChange();

            if (value.Id != -1)
            {
                MyAccName = _availableAccNames.FindThroughTheForestById(value.Id)!;

                SelectedMed = Meds.First();
            }
            else
            {
                // если в комбике магазов выбрали прочерк,
                // то в основном комбике перескакиваем на АЗС только если был выбран какой-то магаз
                if (Shops.Select(s => s.Id).Contains(MyAccName.Id))
                    MyAccName = _availableAccNames.FindThroughTheForestById(272)!; // АЗС
            }

        }
    }

    public List<AccName> Meds { get; set; } = null!;

    private AccName _selectedMed = null!;
    public AccName SelectedMed
    {
        get => _selectedMed;
        set
        {
            if (Equals(value, _selectedMed)) return;
            _selectedMed = value;
            NotifyOfPropertyChange();
            if (value.Id != -1)
            {
                MyAccName = _availableAccNames.FindThroughTheForestById(value.Id)!;
                SelectedShop = Shops.First();
            }
            else
            {
                // если в комбике медов выбрали прочерк,
                // то в основном комбике перескакиваем на АЗС только если был выбран какой-то мед
                if (Meds.Select(s => s.Id).Contains(MyAccName.Id))
                    MyAccName = _availableAccNames.FindThroughTheForestById(272)!; // АЗС
            }
        }
    }

    public string ControlTitle { get; set; } = string.Empty;

    public Visibility Visibility { get; set; }

}
