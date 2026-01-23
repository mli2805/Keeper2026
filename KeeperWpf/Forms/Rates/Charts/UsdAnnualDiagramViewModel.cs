using System;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace KeeperWpf;

public class UsdAnnualDiagramViewModel : Screen
{
    private string _caption = null!;
    private KeeperDataModel _keeperDataModel = null!;

    public PlotModel MyPlotModel2023 { get; set; } = new PlotModel();
    public PlotModel MyPlotModel2024 { get; set; } = new PlotModel();
    public PlotModel MyPlotModel2025 { get; set; } = new PlotModel();
    public PlotModel MyPlotModel2026 { get; set; } = new PlotModel();

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _caption;
    }

    public void Initialize(string caption, KeeperDataModel keeperDataModel)
    {
        _caption = caption;
        _keeperDataModel = keeperDataModel;

        InitializeYear(MyPlotModel2023, 2023);
        InitializeYear(MyPlotModel2024, 2024);
        InitializeYear(MyPlotModel2025, 2025);
        InitializeYear(MyPlotModel2026, 2026);
    }

    private void InitializeYear(PlotModel yearPlotModel, int year)
    {
        yearPlotModel.Series.Add(OneYearOfUsd(year));
        yearPlotModel.Axes.Add(new DateTimeAxis()
        {
            Minimum = DateTimeAxis.ToDouble(new DateTime(year, 1, 1)),
            IntervalLength = 45,
            IntervalType = DateTimeIntervalType.Days,
            MajorGridlineStyle = LineStyle.Solid,
            Maximum = DateTimeAxis.ToDouble(new DateTime(year, 12, 31)),
        });
    }

    private LineSeries OneYearOfUsd(int year)
    {
        var result = new LineSeries() { Title = year.ToString() };
        foreach (var officialRates in _keeperDataModel.OfficialRates.Values.Where(r => r.Date.Year == year))
        {
            var rate = officialRates.Date < new DateTime(2016, 7, 1)
                ? officialRates.NbRates.Usd.Value / 10000
                : officialRates.NbRates.Usd.Value;
            result.Points.Add(new DataPoint(DateTimeAxis.ToDouble(officialRates.Date), rate));
        }
        return result;
    }

}
