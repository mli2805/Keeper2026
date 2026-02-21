using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KeeperWpf;

public class SalaryViewModel(KeeperDataModel dataModel, SalaryChangesRepository salaryChangesRepository) : Screen
{
    private List<SalaryLineModel> _rows = new List<SalaryLineModel>();
    public List<SalaryLineModel> Rows
    {
        get => _rows;
        set
        {
            if (Equals(value, _rows)) return;
            _rows = value;
            NotifyOfPropertyChange();
        }
    }

    private bool _isWithIrregulars;
    private bool _isAggregated;

    private List<SalaryLineModel> _onlySalary = null!;
    private List<SalaryLineModel> _salaryAndIrregulars = null!;

    private PlotModel _myPlotModel = null!;
    public PlotModel MyPlotModel
    {
        get => _myPlotModel;
        set
        {
            if (Equals(value, _myPlotModel)) return;
            _myPlotModel = value;
            NotifyOfPropertyChange();
        }
    }

    private string _toggleButtonCaption = "Add irregulars";
    public string ToggleButtonCaption
    {
        get => _toggleButtonCaption;
        set
        {
            if (value == _toggleButtonCaption) return;
            _toggleButtonCaption = value;
            NotifyOfPropertyChange();
        }
    } // "Only salary";

    private string _aggregateButtonCaption = "Aggregate";
    public string AggregateButtonCaption
    {
        get => _aggregateButtonCaption;
        set
        {
            if (value == _aggregateButtonCaption) return;
            _aggregateButtonCaption = value;
            NotifyOfPropertyChange();
        }
    } // "In details"

    private Visibility _tableVisibility;
    public Visibility TableVisibility
    {
        get => _tableVisibility;
        set
        {
            if (value == _tableVisibility) return;
            _tableVisibility = value;
            NotifyOfPropertyChange(() => TableVisibility);
        }
    }

    public List<SalaryChange> SalaryChanges { get; set; } = null!;
    public List<AccountItemModel> Employers { get; set; } = null!;

    private Visibility _salaryChangesVisibility = Visibility.Collapsed;
    public Visibility SalaryChangesVisibility
    {
        get => _salaryChangesVisibility;
        set
        {
            if (value == _salaryChangesVisibility) return;
            _salaryChangesVisibility = value;
            NotifyOfPropertyChange(() => SalaryChangesVisibility);
        }
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Salary  (O - оклады, T - таблица)";
    }

    public void Initialize()
    {
        SalaryChanges = dataModel.SalaryChanges;
        Employers = dataModel.AcMoDict[171].Children.Cast<AccountItemModel>().ToList();

        var myEmployersFolder = dataModel.AcMoDict[171];
        _onlySalary = BuildFor(myEmployersFolder, false).ToList();
        _salaryAndIrregulars = BuildFor(myEmployersFolder, true).ToList();

        Rows = _salaryAndIrregulars;
        _isWithIrregulars = true;
        _isAggregated = false;

        MyPlotModel = BuildChart();
    }

    private PlotModel BuildChart()
    {
        var myPlotModel = new PlotModel();
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            MajorStep = 12,
            LabelFormatter = F,
            Key = "y1"
        };
        var valueAxis1 = new LinearAxis
        {
            Position = AxisPosition.Left,
            Minimum = 0,
            Key = "x1"
        };
        myPlotModel.Axes.Add(categoryAxis);
        myPlotModel.Axes.Add(valueAxis1);

        CreateBarSeries(myPlotModel);
        return myPlotModel;
    }

    private string F(double valueOnAxys)
    {
        var dateTime = new DateTime(2002, 1, 1).AddMonths((int)valueOnAxys);
        return $"{dateTime:MM/yy}";
    }

    private void CreateBarSeries(PlotModel myPlotModel)
    {
        var aggr = Aggregate(_onlySalary);
        var aggr2 = Aggregate(_salaryAndIrregulars);

        var salarySeries = new BarSeries()
        {
            Title = "Salary",
            FillColor = OxyColors.Blue,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        var irregularSeries = new BarSeries()
        {
            Title = "Irregular",
            FillColor = OxyColors.Gray,
            IsStacked = true,
            XAxisKey = "x1",
            YAxisKey = "y1"
        };
        for (int i = 0; i < aggr.Count; i++)
        {
            salarySeries.Items.Add(new BarItem((double)aggr[i].AmountInUsd));
            irregularSeries.Items.Add(new BarItem((double)(aggr2[i].AmountInUsd - aggr[i].AmountInUsd)));
        }

        myPlotModel.Series.Add(salarySeries);
        myPlotModel.Series.Add(irregularSeries);
    }

    private IEnumerable<SalaryLineModel> BuildFor(AccountItemModel employersFolder, bool includeIrregulars)
    {
        var transactionModels = dataModel.Transactions.Values
            .Where(t => t.Counterparty != null && t.Counterparty.Is(employersFolder));

        if (!includeIrregulars)
        {
            // 772 - официальн зарплата
            transactionModels = transactionModels.Where(t => t.Category!.Is(772));
        }

        return transactionModels.Select(ToSalaryLine);
    }

    private List<SalaryLineModel> Aggregate(List<SalaryLineModel> rows)
    {
        var aggr = (from l in rows
                    group l by new { mm = l.Timestamp.Month, yy = l.Timestamp.Year }
            into ag
                    select new SalaryLineModel
                    {
                        IsAggregatedLine = true,
                        Timestamp = new DateTime(ag.Key.yy, ag.Key.mm, 1),
                        AmountInUsd = ag.Sum(l => l.AmountInUsd)
                    }).ToList();

        foreach (var salaryLineModel in rows)
        {
            var line = aggr.First(l =>
                l.Timestamp.Year == salaryLineModel.Timestamp.Year &&
                l.Timestamp.Month == salaryLineModel.Timestamp.Month);

            line.Employer = salaryLineModel.Employer;
            line.Amount = $"{line.AmountInUsd:0,0} usd";

            if (!string.IsNullOrEmpty(salaryLineModel.Comment))
            {
                if (!string.IsNullOrEmpty(line.Comment))
                    line.Comment = line.Comment + " ; ";
                line.Comment = line.Comment + salaryLineModel.Comment;
            }
        }
        return aggr;
    }

    private SalaryLineModel ToSalaryLine(TransactionModel transaction)
    {
        SalaryLineModel result = new SalaryLineModel();
        result.Timestamp = transaction.Timestamp;
        result.Employer = transaction.Counterparty!.Name;
        result.Amount = dataModel.AmountInUsdString(transaction.Timestamp, transaction.Currency, transaction.Amount, out decimal amountInUsd);
        result.AmountInUsd = amountInUsd;
        result.Comment = transaction.Comment;
        return result;
    }

    public void ToggleView()
    {
        Rows = _isWithIrregulars ? _onlySalary : _salaryAndIrregulars;
        _isWithIrregulars = !_isWithIrregulars;
        ToggleButtonCaption = _isWithIrregulars ? "Only salary" : "Add irregulars";
    }

    private void ShowSalaryChanges()
    {
        SalaryChangesVisibility = SalaryChangesVisibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ShowTable()
    {
        TableVisibility = TableVisibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;
    }

    public void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.O)
        {
            ShowSalaryChanges();
            e.Handled = true;
        }
        else if (e.Key == Key.T)
        {
            ShowTable();
            e.Handled = true;
        }
    }

    public void AggregateButton()
    {
        Rows = _isAggregated
            ? _isWithIrregulars
                ? _salaryAndIrregulars
                : _onlySalary
            : Aggregate(Rows);
        _isAggregated = !_isAggregated;
        AggregateButtonCaption = _isAggregated ? "In details" : "Aggregate";
    }

    public async Task Close()
    {
        await TryCloseAsync();
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await salaryChangesRepository.SaveAll(SalaryChanges);

        return await base.CanCloseAsync(cancellationToken);

    }
}
