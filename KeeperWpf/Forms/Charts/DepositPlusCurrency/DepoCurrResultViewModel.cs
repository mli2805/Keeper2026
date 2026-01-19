using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KeeperWpf;

public class DepoCurrResultViewModel : Screen
{
    #region Binding properties

    public PlotModel MonthlyDepoCurrModel { get; set; } = null!;
    public PlotModel MonthlySaldoModel { get; set; } = null!;
    public PlotModel AnnualSaldoModel { get; set; } = null!;

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
    }

    public void Initialize()
    {
        var points = _depositCurrencySaldoCalculator.Evaluate().ToList();
        MonthlyDepoCurrModel = InitializeMonthlyDepoCurrPlotModel(points);
        MonthlySaldoModel = InitializeMonthlySaldoPlotModel(points);

        var yearPoints = points
           .GroupBy(p => p.StartDate.Year)
           .Select(gr => new DepoCurrencyData()
           {
               StartDate = gr.First().StartDate,
               DepoRevenue = gr.Sum(l => l.DepoRevenue),
               CurrencyRatesDifferrence = gr.Sum(l => l.CurrencyRatesDifferrence),
           })
           .ToList();
        AnnualSaldoModel = InitializeAnnualSaldoPlotModel(yearPoints);
        MonthlyDepoCurrModelVisibility = Visibility.Visible;
    }

    private PlotModel InitializeMonthlyDepoCurrPlotModel(List<DepoCurrencyData> points)
    {
        var plotModel = new PlotModel();
        plotModel.Legends.Add(new Legend()
        {
            LegendPosition = LegendPosition.BottomLeft,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Horizontal,
        });
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = 12,
            LabelFormatter = Month,
            Key = "y1"
        };
        var valueAxis1 = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis1);

        var series = new BarSeries()
        {
            Title = "Monthly Depo",
            FillColor = OxyColors.Blue,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        series.Items.AddRange(points.Select(p => new BarItem((double)p.DepoRevenue)));
        plotModel.Series.Add(series);

        var series2 = new BarSeries()
        {
            Title = "Monthly Currencies",
            FillColor = OxyColors.Green,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        series2.Items.AddRange(points.Select(p => new BarItem((double)p.CurrencyRatesDifferrence)));
        plotModel.Series.Add(series2);

        return plotModel;
    }

    private PlotModel InitializeMonthlySaldoPlotModel(List<DepoCurrencyData> points)
    {
        var plotModel = new PlotModel();
        plotModel.Legends.Add(new Legend()
        {
            LegendPosition = LegendPosition.BottomLeft,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Horizontal,
        });
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = 12,
            LabelFormatter = Month,
            Key = "y1"
        };
        var valueAxis1 = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis1);

        var series = new BarSeries()
        {
            Title = "Monthly Saldo",
            FillColor = OxyColors.Blue,
            NegativeFillColor = OxyColors.Red,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        series.Items.AddRange(points.Select(p => new BarItem((double)p.Saldo)));
        plotModel.Series.Add(series);

        return plotModel;
    }

    private PlotModel InitializeAnnualSaldoPlotModel(List<DepoCurrencyData> yearPoints)
    {
        var plotModel = new PlotModel();
        plotModel.Legends.Add(new Legend()
        {
            LegendPosition = LegendPosition.BottomLeft,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Horizontal,
        });
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = 1,
            LabelFormatter = Year,
            Key = "y1"
        };
        var valueAxis1 = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis1);

        var seriesDepo = new BarSeries()
        {
            Title = "Annual Depo",
            FillColor = OxyColors.Green,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        seriesDepo.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.DepoRevenue)));
        plotModel.Series.Add(seriesDepo);

        var seriesCurrencies = new BarSeries()
        {
            Title = "Annual Currencies",
            FillColor = OxyColors.Orange,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        seriesCurrencies.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.CurrencyRatesDifferrence)));
        plotModel.Series.Add(seriesCurrencies);

        var seriesSaldo = new BarSeries()
        {
            Title = "Annual Saldo",
            FillColor = OxyColors.Blue,
            NegativeFillColor = OxyColors.Red,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        seriesSaldo.Items.AddRange(yearPoints.Select(p => new BarItem((double)p.Saldo)));
        plotModel.Series.Add(seriesSaldo);

        return plotModel;
    }

    private string Month(double valueOnAxys)
    {
        var dateTime = new DateTime(2002, 1, 1).AddMonths((int)valueOnAxys);
        return $"{dateTime:MM/yy}";
    }

    private string Year(double valueOnAxys)
    {
        var dateTime = new DateTime(2002, 1, 1).AddYears((int)valueOnAxys);
        return $"{dateTime:yyyy}";
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
