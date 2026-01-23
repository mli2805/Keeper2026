using System.Windows.Media;

namespace KeeperModels;

public class CardBalanceMemoModel
{
    public int Id { get; set; }
    public AccountItemModel Account { get; set; } = null!;
    public decimal BalanceThreshold { get; set; }
    public decimal CurrentBalance { get; set; }

    public SolidColorBrush RowBackground => BalanceThreshold > CurrentBalance ? Brushes.LightPink : Brushes.Transparent;
    public bool IsSelected { get; set; }
}
