using Caliburn.Micro;
using OxyPlot;
using System.Windows;
using System.Windows.Input;

namespace KeeperWpf;

public class BalancesAndSaldosViewModel : Screen
{
    public PlotModel DailyBalancesModel { get; set; } = null!;

    private Visibility _dailyBalancesModelVisibility = Visibility.Visible;
    public Visibility DailyBalancesModelVisibility
    {
        get => _dailyBalancesModelVisibility;
        set
        {
            if (value == _dailyBalancesModelVisibility) return;
            _dailyBalancesModelVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    public PlotModel MonthlySaldoModel { get; set; } = null!;

    private Visibility _monthlySaldoModelVisibility = Visibility.Collapsed;
    public Visibility MonthlySaldoModelVisibility
    {
        get => _monthlySaldoModelVisibility;
        set
        {
            if (value == _monthlySaldoModelVisibility) return;
            _monthlySaldoModelVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    public PlotModel AnnualSaldoModel { get; set; } = null!;

    private Visibility _annualSaldoModelVisibility = Visibility.Collapsed;
    public Visibility AnnualSaldoModelVisibility
    {
        get => _annualSaldoModelVisibility;
        set
        {
            if (value == _annualSaldoModelVisibility) return;
            _annualSaldoModelVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "T - Toggle chart";
    }

    public void Initialize(KeeperDataModel dataModel)
    {
        var plotModels = new BalanceAndSaldoPlotter().Build(dataModel);
        DailyBalancesModel = plotModels["DailyBalancesModel"];
        MonthlySaldoModel = plotModels["MonthlySaldoModel"];
        AnnualSaldoModel = plotModels["AnnualSaldoModel"];
    }

    private int _model = 1;
    public void ToggleModel(KeyEventArgs e)
    {
        if (e.Key != Key.T) return;

        if (_model == 1)
        {
            _model = 2;
            DailyBalancesModelVisibility = Visibility.Collapsed;
            MonthlySaldoModelVisibility = Visibility.Visible;
            AnnualSaldoModelVisibility = Visibility.Collapsed;
        }
        else if (_model == 2)
        {
            _model = 3;
            DailyBalancesModelVisibility = Visibility.Collapsed;
            MonthlySaldoModelVisibility = Visibility.Collapsed;
            AnnualSaldoModelVisibility = Visibility.Visible;
        }
        else
        {
            _model = 1;
            DailyBalancesModelVisibility = Visibility.Visible;
            MonthlySaldoModelVisibility = Visibility.Collapsed;
            AnnualSaldoModelVisibility = Visibility.Collapsed;
        }
    }
}
