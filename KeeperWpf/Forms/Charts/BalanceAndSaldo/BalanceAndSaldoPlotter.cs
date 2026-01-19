using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace KeeperWpf;

public class BalanceAndSaldoPlotter
{
    public Dictionary<string, PlotModel> Build(KeeperDataModel dataModel)
    {
        var points = new BalanceAndSaldoPointsCollector().CollectPointsForThreeSeries(dataModel);
        var dailyPlotModel = BuildDailyPlotModel(points.DailyBalancesPoints);
        var plots = BuildMonthlyAndAnnualPlotModels(points.MonthlySaldoItems, points.AnnualSaldoItems);

        var result = new Dictionary<string, PlotModel>
        {
            { "DailyBalancesModel", dailyPlotModel },
            { "MonthlySaldoModel", plots.Item1 },
            { "AnnualSaldoModel", plots.Item2 }
        };
        return result;
    }

    private PlotModel BuildDailyPlotModel(List<DataPoint> dailyPlotModelPoints)
    {
        var dailyBalancesModel = new PlotModel();

        dailyBalancesModel.Axes.Add(new DateTimeAxis()
        {
            Position = AxisPosition.Bottom,
            IntervalLength = 75,
            MinorIntervalType = DateTimeIntervalType.Days,
            IntervalType = DateTimeIntervalType.Days,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dash,
        });
        dailyBalancesModel.Axes.Add(new LinearAxis()
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Automatic,
            MinorGridlineStyle = LineStyle.Automatic,
        });

        var dailyBalancesSeries = new LineSeries()
        {
            Title = "Дневные остатки",
            Color = OxyColors.BlueViolet,
        };
        dailyBalancesSeries.Points.AddRange(dailyPlotModelPoints);

        dailyBalancesModel.Series.Add(dailyBalancesSeries);
        return dailyBalancesModel;
    }

    private (PlotModel, PlotModel) BuildMonthlyAndAnnualPlotModels(List<BarItem> monthPlotItems, List<BarItem> annualPlotItems)
    {
        var MonthlySaldoModel = new PlotModel();
        var valueAxis1 = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
        var monthCategoryAxis = new CategoryAxis()
        {
            Position = AxisPosition.Bottom,
            MajorStep = 12,
            LabelFormatter = Month,
            Key = "y1"
        };

        var AnnualSaldoModel = new PlotModel();
        var valueAxis2 = new LinearAxis
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Dash,
            Key = "x1"
        };
        var yearCategoryAxis = new CategoryAxis()
        {
            Position = AxisPosition.Bottom,
            LabelFormatter = Year,
            MajorStep = 1,
            Key = "y1"
        };

        MonthlySaldoModel.Axes.Add(valueAxis1);
        MonthlySaldoModel.Axes.Add(monthCategoryAxis);
        AnnualSaldoModel.Axes.Add(valueAxis2);
        AnnualSaldoModel.Axes.Add(yearCategoryAxis);

        var monthlySaldoSeries = new BarSeries()
        {
            Title = "Сальдо по месяцам",
            FillColor = OxyColors.Blue,
            NegativeFillColor = OxyColors.Red,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        var annualSaldoSeries = new BarSeries()
        {
            Title = "Сальдо по годам",
            FillColor = OxyColors.Blue,
            NegativeFillColor = OxyColors.Red,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };

        monthlySaldoSeries.Items.AddRange(monthPlotItems);
        annualSaldoSeries.Items.AddRange(annualPlotItems);

        MonthlySaldoModel.Series.Add(monthlySaldoSeries);
        AnnualSaldoModel.Series.Add(annualSaldoSeries);
        return (MonthlySaldoModel, AnnualSaldoModel);
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
}