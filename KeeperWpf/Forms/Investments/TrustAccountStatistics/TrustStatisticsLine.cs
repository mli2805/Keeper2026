using System.Windows.Media;
using Caliburn.Micro;
using KeeperModels;

namespace KeeperWpf;

public class TrustStatisticsLine : PropertyChangedBase
{
    public TrustTranModel Tran { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string AmountIn { get; set; } = null!;
    public string AmountOut { get; set; } = null!;
    public string BynFee { get; set; } = null!;
    public decimal BalanceAfter { get; set; }
    public string BalanceAfterStr => IsForWholeTrustAccount
        ? $"{BalanceAfter:#,0.00} {Tran.TrustAccount.Currency.ToString().ToLowerInvariant()}"
        : $"{BalanceAfter:#,0.00} шт";

    public bool IsForWholeTrustAccount;

    public Brush TransBrush { get; set; } = null!;

    private bool _isSelected;
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (value.Equals(_isSelected)) return;
            _isSelected = value;
            NotifyOfPropertyChange(() => IsSelected);
        }
    }

}