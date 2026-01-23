using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace KeeperWpf;

public class BasketDiagramViewModel : Screen
{
    private string _caption = null!;
    private readonly DateTime _startDate = new DateTime(2012, 7, 1);
    private readonly DateTime _startDateD = DateTime.Now.AddMonths(-6);
    private List<OfficialRatesModel> _rates = null!;

    public PlotModel BasketPlotModel { get; set; } = null!;
    public PlotModel BasketDeltaPlotModel { get; set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _caption + "  ;  T - Transform";
    }

    public void Initalize(string caption, List<OfficialRatesModel> rates)
    {
        _caption = caption;

        BasketPlotModel = new PlotModel();
        BasketPlotModel.Legends.Add(new Legend()
        {
            LegendPosition = LegendPosition.TopLeft,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Horizontal,
        });

        BasketDeltaPlotModel = new PlotModel();
        // влияет на Series.Title(s), без этого легенда не будет отрисована,
        // еще можно задать Title для осей, они пишутся рядом с осями
        BasketDeltaPlotModel.Legends.Add(new Legend()
        {
            LegendPosition = LegendPosition.TopCenter,
            LegendPlacement = LegendPlacement.Inside,
            LegendOrientation = LegendOrientation.Horizontal,
        });
        
        BasketPlotModel.Axes.Add(new DateTimeAxis()
        {
            IntervalType = DateTimeIntervalType.Months,
            MajorGridlineStyle = LineStyle.Solid,
        });

        BasketDeltaPlotModel.Axes.Add(new CategoryAxis()
        {
            IsTickCentered = true,
            MajorStep = (DateTime.Today - _startDateD).Days / 12.0,
            LabelFormatter = F,
            Key = "y1"
        });
        BasketDeltaPlotModel.Axes.Add(new LinearAxis()
        {
            Position = AxisPosition.Left,
            MajorGridlineStyle = LineStyle.Solid,
            Key = "x1"
        });

        // можно сначала создать BarSeries указав ему оси (XAxisKey = "x1", YAxisKey = "y1"), а потом добавить оси в модель
        // или наоборот, как здесь сначала оси, потом Series, работает в обоих случаях

        _rates = rates;
        GetLinesOfBasket(out LineSeries basket, out BarSeries delta);
        BasketPlotModel.Series.Add(basket);
        BasketDeltaPlotModel.Series.Add(delta);
    }

    private string F(double day)
    {
        return (_startDateD + TimeSpan.FromDays(day)).ToShortDateString();
    }

    private void GetLinesOfBasket(out LineSeries basket, out BarSeries basketDelta)
    {
        basket = new LineSeries() { Title = "Корзина 30-20-50", TextColor = OxyColors.Orange };

        basketDelta = new BarSeries()
        {
            Title = "Data Дневное изменение корзины в %",
            FillColor = OxyColors.Red,
            NegativeFillColor = OxyColors.Green,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };

        var threshold = new DateTime(2016,7,1);
        foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date >= _startDate))
        {
            var basketValue = nbRbRateOnScreen.Date >= threshold
                ? nbRbRateOnScreen.Basket
                : nbRbRateOnScreen.Basket / 10000;
            basket.Points.Add(new DataPoint(DateTimeAxis.ToDouble(nbRbRateOnScreen.Date), basketValue));
            if (nbRbRateOnScreen.Date >= _startDateD)
                basketDelta.Items.Add(new BarItem(nbRbRateOnScreen.ProcBasketDelta));
        }
    }


    #region Transform chart

    private int _rowB = 1;
    private int _rowSpan = 1;
    private int _columnB;
    private int _columnSpan = 2;

    public int RowB
    {
        get { return _rowB; }
        set
        {
            if (value == _rowB) return;
            _rowB = value;
            NotifyOfPropertyChange();
        }
    }

    public int RowSpan
    {
        get { return _rowSpan; }
        set
        {
            if (value == _rowSpan) return;
            _rowSpan = value;
            NotifyOfPropertyChange();
        }
    }

    public int ColumnB
    {
        get { return _columnB; }
        set
        {
            if (value == _columnB) return;
            _columnB = value;
            NotifyOfPropertyChange();
        }
    }

    public int ColumnSpan
    {
        get { return _columnSpan; }
        set
        {
            if (value == _columnSpan) return;
            _columnSpan = value;
            NotifyOfPropertyChange();
        }
    }

    public void TransformChart(KeyEventArgs e)
    {
        if (e.Key != Key.T) return; 

        RowB = RowB == 1 ? 0 : 1;
        RowSpan = RowSpan == 1 ? 2 : 1;
        ColumnB = ColumnB == 1 ? 0 : 1;
        ColumnSpan = ColumnSpan == 1 ? 2 : 1;
    }

    #endregion
}