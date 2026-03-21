using Caliburn.Micro;
using System.Windows.Media;

namespace KeeperModels;

public class BankAccountMemoModel : PropertyChangedBase
{
    public int Id { get; set; }
    public AccountItemModel Account { get; set; } = null!;

    // если меньше на карточке, то предупреждать чтобы пополнил
    private LimitModel _balanceLess = new();
    public LimitModel BalanceLess
    {
        get => _balanceLess;
        set
        {
            if (_balanceLess == value) return;
            _balanceLess.PropertyChanged -= OnLimitPropertyChanged;
            _balanceLess = value;
            _balanceLess.PropertyChanged += OnLimitPropertyChanged;
            NotifyOfPropertyChange(nameof(RowBackground));
        }
    }

    // если больше на счете/карточке, могут брать комиссию
    private LimitModel _balanceMore = new();
    public LimitModel BalanceMore
    {
        get => _balanceMore;
        set
        {
            if (_balanceMore == value) return;
            _balanceMore.PropertyChanged -= OnLimitPropertyChanged;
            _balanceMore = value;
            _balanceMore.PropertyChanged += OnLimitPropertyChanged;
            NotifyOfPropertyChange(nameof(RowBackground));
        }
    }

    // условия для кэшбека:
    //   иногда требуется, чтобы не меньше определенной суммы было потрачено, иначе кэшбек не будет начислен,
    //   иногда есть ограничение сверху
    private LimitModel _paymentsLess = new();
    public LimitModel PaymentsLess
    {
        get => _paymentsLess;
        set
        {
            if (_paymentsLess == value) return;
            _paymentsLess.PropertyChanged -= OnLimitPropertyChanged;
            _paymentsLess = value;
            _paymentsLess.PropertyChanged += OnLimitPropertyChanged;
            NotifyOfPropertyChange(nameof(RowBackground));
        }
    }

    private LimitModel _paymentsMore = new();
    public LimitModel PaymentsMore
    {
        get => _paymentsMore;
        set
        {
            if (_paymentsMore == value) return;
            _paymentsMore.PropertyChanged -= OnLimitPropertyChanged;
            _paymentsMore = value;
            _paymentsMore.PropertyChanged += OnLimitPropertyChanged;
            NotifyOfPropertyChange(nameof(RowBackground));
        }
    }

    public BankAccountMemoModel()
    {
        BalanceLess.PropertyChanged += OnLimitPropertyChanged;
        BalanceMore.PropertyChanged += OnLimitPropertyChanged;
        PaymentsLess.PropertyChanged += OnLimitPropertyChanged;
        PaymentsMore.PropertyChanged += OnLimitPropertyChanged;
    }

    private void OnLimitPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        NotifyOfPropertyChange(nameof(IsLimitExceeded));
        NotifyOfPropertyChange(nameof(RowBackground));
    }

    // лимиты на пополнение/снятие наличных, по ЕРИП, пока пишем текстом 
    public string Comment { get; set; } = string.Empty;


    // рассчетные поля, не сохраняются, нужны для отображения в таблице
    public decimal CurrentBalance { get; set; }
    public decimal CurrentMonthPayments { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(RowBackground));
        }
    }

    public bool IsLimitExceeded => CheckIsLimitExceeded();
    public SolidColorBrush RowBackground => IsSelected
            ? IsLimitExceeded ? Brushes.LightCoral : (SolidColorBrush)new BrushConverter().ConvertFrom("#B6E6FF")!
            : IsLimitExceeded ? Brushes.LightPink : Brushes.Transparent;

    private bool CheckIsLimitExceeded()
    {
        if (BalanceLess.IsOn && CurrentBalance < BalanceLess.LimitValue)
        {
            return true;
        }
        if (BalanceMore.IsOn && CurrentBalance > BalanceMore.LimitValue)
        {
            return true;
        }
        if (PaymentsLess.IsOn && CurrentMonthPayments < PaymentsLess.LimitValue)
        {
            return true;
        }
        if (PaymentsMore.IsOn && CurrentMonthPayments > PaymentsMore.LimitValue)
        {
            return true;
        }
        return false;
    }
}

public class LimitModel : PropertyChangedBase
{
    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (value == _isOn) return;
            _isOn = value;
            NotifyOfPropertyChange();
            if (!value)
            {
                LimitValue = 0;
            }
        }
    }

    private int _limitValue;
    public int LimitValue
    {
        get => _limitValue;
        set
        {
            if (value == _limitValue) return;
            _limitValue = value;
            NotifyOfPropertyChange();
        }
    }
}