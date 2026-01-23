using System;
using System.Windows.Media;
using KeeperDomain;

namespace KeeperWpf;

public class DepositVm
{
    public int Id { get; set; }
    public bool IsSelected { get; set; }
    public string BankName { get; set; } = null!;
    public string DepoName { get; set; } = null!;

    public CurrencyCode MainCurrency { get; set; }

    public RateType RateType { get; set; }
    public string RateFormula { get; set; } = string.Empty;

    public string RateTypeStr =>
        RateType == RateType.Fixed ? "фикс" : RateType == RateType.Floating ? "плав" : RateFormula;

    public decimal Rate { get; set; }
    public string AdditionsStr { get; set; } = null!;
    public bool IsAddOpen { get; set; }

    public Brush BackgroundBrush => FinishDate < DateTime.Today
        ? Brushes.LightPink
        : IsAddOpen
            ? Brushes.PaleGreen
            : Brushes.LightGray;

    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }

    public Balance Balance { get; set; } = null!;
}