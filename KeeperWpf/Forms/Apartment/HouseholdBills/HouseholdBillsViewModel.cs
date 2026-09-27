using Caliburn.Micro;
using KeeperModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KeeperWpf;

/// <summary>
/// Utility bills — счета за коммунальные услуги.
/// Household bills — все счета по дому (включая интернет и телефон).
/// 
/// В дереве счетов (к дереву можно обращаться _dataModel.AccountsTree или _dataModel.AcMoDict) 
/// папка Коммунальные платежи ID=192 содержит счета, по которым отражается все коммунальные платежи за квартиру.
/// Среди транзакций надо выбрать те, что проходят по этим счетам и отобразить их в таблице и чартом.
/// Переключение между ними по клавише T.
/// 
/// C 1/07/2016 года в Беларуси произошла деноминация, поэтому суммы в BYR и BYN отличаются на 10000 раз.
/// Приведи все суммы к BYN, 2 знака после запятой, и отображай в таблице и на чарте суммы в BYN.
/// 
/// В таблице должны быть следующие колонки: 
/// Дата, Счет (название счета, по которому прошла транзакция), Сумма, Сумма в $, коментарий из транзакции. 
/// (для перевода суммы операции в доллары можно воспользоваться public static class AmountInUsdProvider,
/// либо, т.к. обычно несколько коммунальных платежей идет в 1 день, 
/// то можно взять курс public double BynToUsd { get; set; } из ExchangeRates и посчитать - подумай, что эффективнее).
/// В таблице сделать чередование цвета фона строк (например, белый и светло-серый) 
/// - месяц одним цветом, следующий месяц другим цветом. 
/// Предусмотреть возможность переключения между отображение всех операций раздельно
/// и отображением всего месяца/года в одной строке (внизу таблицы радиобаттоны: раздельно/по месяцам/по годам).
/// При загрузке скрол вниз, к свежим оплатам.
/// 
/// Чарт рисовать с помощью OxyPlot, столбцовую диаграмму, по оси X - даты, по оси Y - сумма.
/// предусмотреть возможность переключения между отображением суммы в BYN и в USD по клавише C,
/// а также возможность переключения между отображением сумм по годам и по месяцам.
/// Рабочий пример столбцовой диаграммы с OxyPlot можно посмотреть в DepoCurrResultViewModel.cs
/// 
/// Добавить в чарте выбор между отображением сумм по всем счетам вместе и по каждому счету отдельно,
/// радиобаттоны (все/жировка/газ/электричество и т.п.) в верхнем левом углу чарта.
/// Перенесем эту радиогруппу вниз слева в ту же строку где радиогруппа группировки месяц-год, а месяц-год прижмем вправо
/// 
/// Иногда по счету нету платежей несколько месяцев подряд, соответственно на графике нету столбцов и 
/// нижняя ось (время) искажается. Надо для таких случаев вставлять столбец со значением 0. При агрегировании по годам тоже
/// </summary>

[ExportViewModel]
public class HouseholdBillsViewModel : Screen
{
    private const int HouseholdBillsCategoryId = 192;
    private static readonly DateTime BynIntroductionDate = new(2016, 7, 1);

    private readonly KeeperDataModel _dataModel;
    private IReadOnlyList<HouseholdBillRow> _allRows = [];
    private HouseholdBillsTableMode _tableMode = HouseholdBillsTableMode.Separate;
    private HouseholdBillsChartInterval _chartInterval = HouseholdBillsChartInterval.Monthly;
    private bool _isUsdChart;
    private bool _isChartVisible;
    private PlotModel _chartModel = new();
    private int? _selectedChartAccountId;

    public BindableCollection<HouseholdBillRow> Rows { get; } = new();
    public BindableCollection<HouseholdBillChartAccountOption> ChartAccountOptions { get; } = new();

    public Visibility TableVisibility => _isChartVisible ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ChartVisibility => _isChartVisible ? Visibility.Visible : Visibility.Collapsed;

    public PlotModel ChartModel
    {
        get => _chartModel;
        private set
        {
            _chartModel = value;
            NotifyOfPropertyChange();
        }
    }

    public void ToggleView(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.T:
                _isChartVisible = !_isChartVisible;
                NotifyOfPropertyChange(nameof(TableVisibility));
                NotifyOfPropertyChange(nameof(ChartVisibility));
                break;
            case Key.C:
                _isUsdChart = !_isUsdChart;
                RebuildChart();
                break;
        }
    }

    public bool IsMonthlyChartMode
    {
        get => _chartInterval == HouseholdBillsChartInterval.Monthly;
        set
        {
            if (value)
                SetChartInterval(HouseholdBillsChartInterval.Monthly);
        }
    }

    public bool IsAnnualChartMode
    {
        get => _chartInterval == HouseholdBillsChartInterval.Annual;
        set
        {
            if (value)
                SetChartInterval(HouseholdBillsChartInterval.Annual);
        }
    }

    public bool IsSeparateTableMode
    {
        get => _tableMode == HouseholdBillsTableMode.Separate;
        set
        {
            if (value)
                SetTableMode(HouseholdBillsTableMode.Separate);
        }
    }

    public bool IsMonthlyTableMode
    {
        get => _tableMode == HouseholdBillsTableMode.Monthly;
        set
        {
            if (value)
                SetTableMode(HouseholdBillsTableMode.Monthly);
        }
    }

    public bool IsAnnualTableMode
    {
        get => _tableMode == HouseholdBillsTableMode.Annual;
        set
        {
            if (value)
                SetTableMode(HouseholdBillsTableMode.Annual);
        }
    }

    public HouseholdBillsViewModel(KeeperDataModel dataModel)
    {
        _dataModel = dataModel;
        DisplayName = "Счета за коммунальные услуги. T - таблица/график, C - BYN/USD";
    }

    public void Initialize()
    {
        var householdBillsCategory = _dataModel.AcMoDict[HouseholdBillsCategoryId];
        var chartAccounts = householdBillsCategory.Children
            .OfType<AccountItemModel>()
            .OrderBy(account => account.ChildNumber)
            .ToList();

        ChartAccountOptions.Clear();
        ChartAccountOptions.Add(new HouseholdBillChartAccountOption(
            null,
            "Все",
            SelectChartAccount));
        ChartAccountOptions.AddRange(chartAccounts.Select(account => new HouseholdBillChartAccountOption(
            account.Id,
            account.Name,
            SelectChartAccount)));
        ChartAccountOptions[0].IsSelected = true;

        _allRows = _dataModel.Transactions.Values
            .Where(transaction => transaction.Category?.Is(HouseholdBillsCategoryId) == true)
            .OrderBy(transaction => transaction.Timestamp)
            .ThenBy(transaction => transaction.Id)
            .Select(transaction => new HouseholdBillRow
            {
                Date = transaction.Timestamp,
                AccountName = transaction.Category!.Name,
                ChartAccountId = FindChartAccountId(transaction.Category, chartAccounts),
                Amount = NormalizeAmountInByn(transaction.Timestamp, transaction.Amount),
                AmountInUsd = transaction.GetAmountInUsd(_dataModel),
                Comment = transaction.Comment
            })
            .ToList();

        RebuildTableRows();
        RebuildChart();
    }

    private static int? FindChartAccountId(
        AccountItemModel category,
        IEnumerable<AccountItemModel> chartAccounts)
    {
        return chartAccounts.FirstOrDefault(category.Is)?.Id;
    }

    private void SelectChartAccount(HouseholdBillChartAccountOption selectedOption)
    {
        foreach (var option in ChartAccountOptions.Where(option => option != selectedOption))
            option.IsSelected = false;

        _selectedChartAccountId = selectedOption.AccountId;
        RebuildChart();
    }

    private static decimal NormalizeAmountInByn(DateTime date, decimal amount)
    {
        var amountInByn = date.Date < BynIntroductionDate ? amount / 10_000m : amount;
        return decimal.Round(amountInByn, 2, MidpointRounding.AwayFromZero);
    }

    private void SetTableMode(HouseholdBillsTableMode mode)
    {
        if (_tableMode == mode)
            return;

        _tableMode = mode;
        NotifyOfPropertyChange(nameof(IsSeparateTableMode));
        NotifyOfPropertyChange(nameof(IsMonthlyTableMode));
        NotifyOfPropertyChange(nameof(IsAnnualTableMode));
        RebuildTableRows();
    }

    private void RebuildTableRows()
    {
        IEnumerable<HouseholdBillRow> rows = _tableMode switch
        {
            HouseholdBillsTableMode.Monthly => AggregateRows(row => new DateTime(row.Date.Year, row.Date.Month, 1)),
            HouseholdBillsTableMode.Annual => AggregateRows(row => new DateTime(row.Date.Year, 1, 1)),
            _ => _allRows
        };

        Rows.Clear();
        Rows.AddRange(rows);
    }

    private IEnumerable<HouseholdBillRow> AggregateRows(Func<HouseholdBillRow, DateTime> periodSelector)
    {
        return _allRows
            .GroupBy(periodSelector)
            .OrderBy(group => group.Key)
            .Select(group => new HouseholdBillRow
            {
                Date = group.Key,
                Amount = group.Sum(row => row.Amount),
                AmountInUsd = group.Sum(row => row.AmountInUsd)
            });
    }

    private void SetChartInterval(HouseholdBillsChartInterval interval)
    {
        if (_chartInterval == interval)
            return;

        _chartInterval = interval;
        NotifyOfPropertyChange(nameof(IsMonthlyChartMode));
        NotifyOfPropertyChange(nameof(IsAnnualChartMode));
        RebuildChart();
    }

    private static List<(DateTime Date, decimal Amount)> BuildChartPoints(
        IEnumerable<HouseholdBillRow> rows,
        Func<HouseholdBillRow, DateTime> periodSelector,
        Func<DateTime, DateTime> nextPeriodSelector,
        Func<HouseholdBillRow, decimal> amountSelector)
    {
        var amountsByPeriod = rows
            .GroupBy(periodSelector)
            .ToDictionary(group => group.Key, group => group.Sum(amountSelector));

        if (amountsByPeriod.Count == 0)
            return [];

        var lastPeriod = amountsByPeriod.Keys.Max();
        var points = new List<(DateTime Date, decimal Amount)>();
        for (var period = amountsByPeriod.Keys.Min();; period = nextPeriodSelector(period))
        {
            var amount = amountsByPeriod.ContainsKey(period) ? amountsByPeriod[period] : 0m;
            points.Add((period, amount));
            if (period == lastPeriod)
                return points;
        }
    }

    private void RebuildChart()
    {
        Func<HouseholdBillRow, DateTime> periodSelector = _chartInterval == HouseholdBillsChartInterval.Monthly
            ? row => new DateTime(row.Date.Year, row.Date.Month, 1)
            : row => new DateTime(row.Date.Year, 1, 1);
        Func<DateTime, DateTime> nextPeriodSelector = _chartInterval == HouseholdBillsChartInterval.Monthly
            ? period => period.AddMonths(1)
            : period => period.AddYears(1);

        var chartRows = _selectedChartAccountId.HasValue
            ? _allRows.Where(row => row.ChartAccountId == _selectedChartAccountId)
            : _allRows;

        var points = BuildChartPoints(
            chartRows,
            periodSelector,
            nextPeriodSelector,
            _isUsdChart ? row => row.AmountInUsd : row => row.Amount);

        var currencyName = _isUsdChart ? "USD" : "BYN";
        var model = new PlotModel
        {
            Title = $"Коммунальные платежи, {currencyName}"
        };
        var categoryAxis = new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            Key = "Periods",
            Angle = points.Count > 24 ? -90 : -45
        };
        for (var index = 0; index < points.Count; index++)
        {
            var point = points[index];
            var label = _chartInterval == HouseholdBillsChartInterval.Monthly
                ? index % 4 == 0 ? point.Date.ToString("MM/yyyy") : string.Empty
                : point.Date.ToString("yyyy");
            categoryAxis.Labels.Add(label);
        }

        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Key = "Amounts",
            MinimumPadding = 0,
            AbsoluteMinimum = 0,
            MajorGridlineStyle = LineStyle.Dash,
            StringFormat = "#,0.00",
            Title = currencyName
        };
        var series = new BarSeries
        {
            Title = currencyName,
            FillColor = OxyColors.SteelBlue,
            XAxisKey = "Amounts",
            YAxisKey = "Periods",
            TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:N2}"
        };
        series.Items.AddRange(points.Select(point => new BarItem((double)point.Amount)));

        model.Axes.Add(categoryAxis);
        model.Axes.Add(valueAxis);
        model.Series.Add(series);
        ChartModel = model;
    }
}
