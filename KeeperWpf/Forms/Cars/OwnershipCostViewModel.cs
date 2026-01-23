using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using KeeperModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace KeeperWpf;

public class OwnershipCostViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private CarModel _carModel = null!;

    public PlotModel MonthlyOwnershipCostPlotModel { get; set; } = null!;
    public PlotModel AnnualOwnershipCostPlotModel { get; set; } = null!;

    private Visibility _monthlyVisibility = Visibility.Collapsed;
    public Visibility MonthlyVisibility
    {
        get => _monthlyVisibility;
        set
        {
            if (value == _monthlyVisibility) return;
            _monthlyVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    private Visibility _yearVisibility = Visibility.Visible;
    public Visibility YearVisibility
    {
        get => _yearVisibility;
        set
        {
            if (value == _yearVisibility) return;
            _yearVisibility = value;
            NotifyOfPropertyChange();
        }
    }


    public OwnershipCostViewModel(KeeperDataModel dataModel)
    {
        _dataModel = dataModel;
    }

    public void Initialize(CarModel carModel)
    {
        _carModel = carModel;

        var carAccountModel = _dataModel.AcMoDict[_carModel.CarAccountId];

        // покупка, продажа, обмен авто не учитываются
        var buySellIds = new List<int> { 707, 709, 713, 717 };
        var trans = _dataModel.Transactions.Values.OrderBy(t => t.Timestamp)
            .Where(m => m.Category != null && m.Category.Is(carAccountModel) && !buySellIds.Contains(m.Category.Id)).ToList();

        MonthlyOwnershipCostPlotModel = InitializePlot(trans, carModel.PurchaseDate, "month");
        AnnualOwnershipCostPlotModel = InitializePlot(trans, carModel.PurchaseDate, "year");
    }

    private PlotModel InitializePlot(List<TransactionModel> trans, DateTime purchaseDate, string period)
    {
        var plotModel = new PlotModel();
        var barSeries = new BarSeries
        {
            Title = period == "year" ? "Расходы за год" : "Расходы за месяц",
            FillColor = OxyColors.SteelBlue,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        var lineSeries = new LineSeries
        {
            Title = "Среднее значение",
            Color = OxyColors.Red,
            MarkerType = MarkerType.Circle,
        };
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = period == "year" ? 1 : 5,
            Key = "y1"
        };
        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
       

        // вычисление
        DateTime currentDate = purchaseDate.Date;
        var yearCount = 0;
        var totalSum = 0m;
        int columnIndex = 0;
        do
        {
            DateTime nextPeriodStart = period == "year" ? currentDate.AddYears(1) : currentDate.AddMonths(1);
            decimal sumInUsd = 0;

            var yearTrans = trans
                .Where(t => t.Timestamp.Date >= currentDate.Date && t.Timestamp.Date < nextPeriodStart.Date)
                .ToList();
            foreach (var transaction in yearTrans)
            {
                sumInUsd += transaction.GetAmountInUsd(_dataModel);
            }
            var item = new BarItem((double)sumInUsd);
            barSeries.Items.Add(item);
            var label = period == "year" 
                ? currentDate.ToString("dd/MM/yy") + "-" + currentDate.AddYears(1).AddDays(-1).ToString("dd/MM/yy")
                : currentDate.ToString("MMMyy");
            categoryAxis.Labels.Add(label);
            columnIndex++;

            currentDate = period == "year" ? currentDate.AddYears(1) : currentDate.AddMonths(1);

            totalSum += sumInUsd;
            yearCount++;
            var average = totalSum / yearCount;
            lineSeries.Points.Add(new DataPoint(yearCount - 1, (double)average));
        }
        while (currentDate <= trans.Last().Timestamp.Date);

        plotModel.Series.Add(barSeries);
        plotModel.Series.Add(lineSeries);
        plotModel.Axes.Add(categoryAxis);
        plotModel.Axes.Add(valueAxis);
        return plotModel;
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Стоимость владения {_carModel.Title}  (T - переключить день/месяц)";
    }



    private int _model = 1;
    public void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.T) return;

        switch (_model)
        {
            case 1:
                _model = 2;
                YearVisibility = Visibility.Collapsed;
                MonthlyVisibility = Visibility.Visible;
                break;
            default:
                _model = 1;
                YearVisibility = Visibility.Visible;
                MonthlyVisibility = Visibility.Collapsed;
                break;
        }

    }
}
