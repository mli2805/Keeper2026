using Caliburn.Micro;

namespace KeeperModels;

public class BankAccountMemoModel
{
    public int Id { get; set; }
    public AccountItemModel Account { get; set; } = null!;

    // если меньше на карточке, то предупреждать чтобы пополнил
    public LimitModel BalanceLess { get; set; } = new();
    // если больше на счете/карточке, могут брать комиссию
    public LimitModel BalanceMore { get; set; } = new();

    // условия для кэшбека:
    //   иногда требуется, чтобы не меньше определенной суммы было потрачено, иначе кэшбек не будет начислен,
    //   иногда есть ограничение сверху
    public LimitModel PaymentsLess { get; set; } = new();
    public LimitModel PaymentsMore { get; set; } = new();

    // лимиты на пополнение/снятие наличных, по ЕРИП, пока пишем текстом 
    public string Comment { get; set; } = string.Empty;


    // рассчетные поля, не сохраняются, нужны для отображения в таблице
    public decimal CurrentBalance { get; set; }
    public decimal CurrentMonthPayments { get; set; }

    public bool IsSelected { get; set; }
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