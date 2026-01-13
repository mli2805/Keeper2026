using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KeeperWpf;

public class DepoCurrResultViewModel : Screen
{
    #region Binding properties

    private PlotModel _monthlyDepoCurrModel;
    private PlotModel _monthlySaldoModel;
    private PlotModel _annualSaldoModel;
    public PlotModel MonthlyDepoCurrModel
    {
        get => _monthlyDepoCurrModel;
        set
        {
            if (Equals(value, _monthlyDepoCurrModel)) return;
            _monthlyDepoCurrModel = value;
            NotifyOfPropertyChange();
        }
    }

    public PlotModel MonthlySaldoModel
    {
        get => _monthlySaldoModel;
        set
        {
            if (Equals(value, _monthlySaldoModel)) return;
            _monthlySaldoModel = value;
            NotifyOfPropertyChange();
        }
    }

    public PlotModel AnnualSaldoModel
    {
        get => _annualSaldoModel;
        set
        {
            if (Equals(value, _annualSaldoModel)) return;
            _annualSaldoModel = value;
            NotifyOfPropertyChange();
        }
    }

    private Visibility _monthlyDepoCurrModelVisibility = Visibility.Collapsed;
    private Visibility _monthlySaldoModelVisibility = Visibility.Collapsed;
    private Visibility _annualSaldoModelVisibility = Visibility.Collapsed;
    public Visibility MonthlyDepoCurrModelVisibility
    {
        get => _monthlyDepoCurrModelVisibility;
        set
        {
            if (value == _monthlyDepoCurrModelVisibility) return;
            _monthlyDepoCurrModelVisibility = value;
            NotifyOfPropertyChange();
        }
    }

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
    #endregion

    private readonly DepositCurrencySaldoCalculator _depositCurrencySaldoCalculator;

    public DepoCurrResultViewModel(DepositCurrencySaldoCalculator depositCurrencySaldoCalculator)
    {
        _depositCurrencySaldoCalculator = depositCurrencySaldoCalculator;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "T - Toggle chart";
        using (new WaitCursor())
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        var points = _depositCurrencySaldoCalculator.Evaluate().ToList();
        InitializeMonthlyDepoCurrPlotModel(points);
        InitializeMonthlySaldoPlotModel(points);
        InitializeAnnualSaldoPlotModel(points);
        MonthlyDepoCurrModelVisibility = Visibility.Visible;
    }

    private void InitializeMonthlyDepoCurrPlotModel(List<DepoCurrencyData> points)
    {
        MonthlyDepoCurrModel = new PlotModel();
        MonthlyDepoCurrModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });

        var series = new BarSeries() { Title = "Monthly Depo", FillColor = OxyColors.Blue, IsStacked = true, };
        series.Items.AddRange(points.Select(p => new BarItem((double)p.DepoRevenue)));
        MonthlyDepoCurrModel.Series.Add(series);

        var series2 = new BarSeries() { Title = "Monthly Currencies", FillColor = OxyColors.Green, IsStacked = true };
        series2.Items.AddRange(points.Select(p => new BarItem((double)p.CurrencyRatesDifferrence)));
        MonthlyDepoCurrModel.Series.Add(series2);

        MonthlyDepoCurrModel.Axes.Add(new CategoryAxis() { ItemsSource = points.Select(p => p.Label).ToArray() });
    }

    private void InitializeMonthlySaldoPlotModel(List<DepoCurrencyData> points)
    {
        MonthlySaldoModel = new PlotModel();
        MonthlySaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });

        var series = new BarSeries() { Title = "Monthly Saldo", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
        series.Items.AddRange(points.Select(p => new BarItem((double)p.Saldo)));
        MonthlySaldoModel.Series.Add(series);

        MonthlySaldoModel.Axes.Add(new CategoryAxis() { ItemsSource = points.Select(p => p.Label).ToArray() });
    }

    private void InitializeAnnualSaldoPlotModel(List<DepoCurrencyData> points)
    {
        AnnualSaldoModel = new PlotModel();
        AnnualSaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });

        var yearPoints = points
            .GroupBy(p => p.StartDate.Year)
            .Select(gr => new DepoCurrencyData()
            {
                StartDate = gr.First().StartDate,
                DepoRevenue = gr.Sum(l => l.DepoRevenue),
                CurrencyRatesDifferrence = gr.Sum(l => l.CurrencyRatesDifferrence),
            })
            .ToList();


        var seriesDepo = new BarSeries() { Title = "Annual Depo", FillColor = OxyColors.Green, IsStacked = true, };
        seriesDepo.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.DepoRevenue)));
        AnnualSaldoModel.Series.Add(seriesDepo);

        var seriesCurrencies = new BarSeries() { Title = "Annual Currencies", FillColor = OxyColors.Orange, IsStacked = true };
        seriesCurrencies.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.CurrencyRatesDifferrence)));
        AnnualSaldoModel.Series.Add(seriesCurrencies);

        var seriesSaldo = new BarSeries() { Title = "Annual Saldo", FillColor = OxyColors.Blue, NegativeFillColor = OxyColors.Red };
        seriesSaldo.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.Saldo)));
        AnnualSaldoModel.Series.Add(seriesSaldo);

        AnnualSaldoModel.Axes.Add(new CategoryAxis() { ItemsSource = yearPoints.Select(p => p.Label).ToArray() });
    }


    private int _model = 1;

    public void ToggleModel(KeyEventArgs e)
    {
        if (e.Key != Key.T) return;
        
        if (_model == 1)
        {
            _model = 2;
            MonthlyDepoCurrModelVisibility = Visibility.Collapsed;
            MonthlySaldoModelVisibility = Visibility.Visible;
            AnnualSaldoModelVisibility = Visibility.Collapsed;
        }
        else if (_model == 2)
        {
            _model = 3;
            MonthlyDepoCurrModelVisibility = Visibility.Collapsed;
            MonthlySaldoModelVisibility = Visibility.Collapsed;
            AnnualSaldoModelVisibility = Visibility.Visible;
        }
        else
        {
            _model = 1;
            MonthlyDepoCurrModelVisibility = Visibility.Visible;
            MonthlySaldoModelVisibility = Visibility.Collapsed;
            AnnualSaldoModelVisibility = Visibility.Collapsed;
        }
    }
}
