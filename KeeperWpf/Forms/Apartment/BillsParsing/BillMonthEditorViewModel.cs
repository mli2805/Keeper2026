using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;

namespace KeeperWpf;

public sealed class BillMonthEditorViewModel : PropertyChangedBase
{
    private readonly KeeperDataModel _dataModel;
    private readonly BillMonthJsonStore _store;
    private readonly Dictionary<DateTime, MonthState> _states = new();
    private DateTime? _selectedPeriod;
    private BillEditableRow? _selectedRow;
    private bool _isBusy;
    private string _status = "Выберите месяц для корректировки.";
    public BillMonthEditorViewModel(KeeperDataModel dataModel, BillMonthJsonStore? store = null,
        BindableCollection<string>? serviceNames = null)
    {
        _dataModel = dataModel;
        _store = store ?? new BillMonthJsonStore();
        ServiceNames = serviceNames ?? new BindableCollection<string>();
        if (Directory.Exists(_store.StorageDirectoryPath))
            foreach (var file in Directory.EnumerateFiles(_store.StorageDirectoryPath, "*.json"))
                if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(file), "yyyyMM",
                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var period))
                {
                    try
                    {
                        SetState(_store.Load(period));
                        Periods.Add(period);
                    }
                    catch (Exception exception) { Status = $"Ошибка чтения {Path.GetFileName(file)}: {exception.Message}"; }
                }
        UpdateServiceNames(Array.Empty<string>());
        if (Periods.Count > 0) SelectedPeriod = Periods.OrderBy(period => period).First();
    }
    public BindableCollection<DateTime> Periods { get; } = new();
    public BindableCollection<string> ServiceNames { get; }
    public DateTime? SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (_selectedPeriod == value || IsBusy) return;
            ChangePeriod(value);
        }
    }
    public Func<bool>? CommitPendingEdits { get; set; }

    private async void ChangePeriod(DateTime? value)
    {
        await SelectPeriodAsync(value);
    }

    public async Task PreviousMonth()
    {
        if (CanPreviousMonth)
            await SelectPeriodAsync(SelectedPeriod!.Value.AddMonths(-1));
    }

    public async Task NextMonth()
    {
        if (CanNextMonth)
            await SelectPeriodAsync(SelectedPeriod!.Value.AddMonths(1));
    }

    public async Task<bool> SelectPeriodAsync(DateTime? value)
    {
        if (_selectedPeriod == value) return true;
        if (!await SaveChangesAsync())
        {
            NotifyOfPropertyChange(nameof(SelectedPeriod));
            return false;
        }
        _selectedPeriod = value;
        if (value is { } period && !_states.ContainsKey(period))
            SetState(new(1, period, Array.Empty<ParsedBillRow>(), new(period)));
        SelectedRow = null;
        Refresh(true);
        return true;
    }

    public async Task<bool> SaveChangesAsync()
    {
        if (IsBusy) { Status = "Дождитесь завершения сохранения месяца."; return false; }
        if (SelectedPeriod is not { } period) return true;
        if (CommitPendingEdits?.Invoke() == false || !CanPersistMonth)
        {
            Status = "Месяц не сохранен. Исправьте номера, числа и названия услуг.";
            return false;
        }
        if (!_states[period].Dirty && File.Exists(_store.GetPath(period))) return true;
        return await PersistMonth();
    }
    public BindableCollection<BillEditableRow>? Rows => SelectedPeriod is { } period ? _states[period].Rows : null;
    public ICollectionView? SortedRows => SelectedPeriod is { } period ? _states[period].View : null;
    public BillEditableFooter? Footer => SelectedPeriod is { } period ? _states[period].Footer : null;
    public UtilityMeters? UtilityMeters => SelectedPeriod is { } period ? _states[period].UtilityMeters : null;
    public BillEditableRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            _selectedRow = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanDeleteRow));
            NotifyOfPropertyChange(nameof(CanCopyRow));
        }
    }
    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; NotifyOfPropertyChange(); Refresh(); }
    }
    public string Status
    {
        get => _status;
        private set { _status = value; NotifyOfPropertyChange(); }
    }
    public bool IsReadOnly => IsBusy;
    public bool CanEdit => !IsReadOnly && SelectedPeriod is not null;
    public bool CanSelectMonth => !IsBusy;
    public bool CanPreviousMonth => CanSelectMonth && SelectedPeriod is { } period &&
        period > DateTime.MinValue && Periods.Contains(period.AddMonths(-1));
    public bool CanNextMonth => CanSelectMonth && SelectedPeriod is { } period &&
        period < new DateTime(9999, 12, 1) && Periods.Contains(period.AddMonths(1));
    public bool CanPersistMonth => CanEdit && Rows!.All(row => row.IsValid) && Footer!.IsValid;
    public bool CanLoadMonth => !IsBusy && SelectedPeriod is { } period && File.Exists(_store.GetPath(period));
    public bool CanAddRow => CanEdit;
    public bool CanCopyRow => CanEdit && SelectedRow is not null && Rows!.Contains(SelectedRow);
    public bool CanFillFromPrevious => CanEdit && Rows!.Count > 0;
    public bool CanRepeatMonth => CanEdit;
    public bool CanRecalculate => CanEdit && Rows!.Count > 0;
    public bool CanDeleteRow => CanEdit && SelectedRow is not null;
    public string ColumnTotal => Rows is null ? "" : $"{Rows.Where(row =>
        BillEditableFields.TryNumber(row.Total, out _)).Sum(row => BillEditableFields.Number(row.Total) ?? 0):N2}";
    public string ValidationMessage => SelectedPeriod is null || IsReadOnly ? "" : CanPersistMonth ? "" :
        "Проверьте номера (целые числа от 1), суммы и названия услуг. Неверный ввод блокирует сохранение.";

    public string PaymentReference
    {
        get
        {
            if (SelectedPeriod is not { } period || _dataModel.Transactions is null) return "Платежи недоступны.";
            var from = period.AddMonths(1);
            var to = from.AddMonths(1);
            var payments = _dataModel.Transactions.Values.Where(transaction => transaction.Timestamp >= from &&
                    transaction.Timestamp < to && string.Equals(transaction.Category?.Name, "По жировке", StringComparison.OrdinalIgnoreCase))
                .OrderBy(transaction => transaction.Timestamp).Select(transaction =>
                    $"{transaction.Timestamp:dd.MM.yyyy}: {transaction.Amount:N2} {transaction.Currency}").ToArray();
            return payments.Length == 0 ? "В следующем месяце платежей «По жировке» не найдено." :
                "Платежи следующего месяца (справочно): " + string.Join("; ", payments);
        }
    }
    public void Initialize(IEnumerable<ParsedBillRow> rows, IEnumerable<BillFooter> footers)
    {
        var rowGroups = rows.GroupBy(row => row.Period).ToDictionary(group => group.Key, group => group.ToArray());
        var summaries = footers.GroupBy(footer => footer.Period).ToDictionary(group => group.Key, group => group.First());
        foreach (var period in rowGroups.Keys.Concat(summaries.Keys).Distinct().OrderBy(period => period))
        {
            if (_states.TryGetValue(period, out var state) && state.Dirty) continue;
            rowGroups.TryGetValue(period, out var monthRows);
            summaries.TryGetValue(period, out var summary);
            SetState(new(1, period, monthRows ?? Array.Empty<ParsedBillRow>(), summary ?? new(period),
                GetSourceFile(period, monthRows)));
            if (!Periods.Contains(period)) Periods.Add(period);
        }
        var sorted = Periods.OrderBy(period => period).ToArray();
        Periods.Clear();
        Periods.AddRange(sorted);
        UpdateServiceNames(rows.Select(row => row.ServiceName));
        if (SelectedPeriod is null && Periods.Count > 0) SelectedPeriod = Periods[0];
        Refresh(true);
    }
    private void SetState(BillMonthDocument document)
    {
        var state = new MonthState
        {
            UtilityMeters = BillMonthJsonStore.GetUtilityMeters(document),
            Address = document.Address,
            AccountNumber = document.AccountNumber,
            SourceFile = string.IsNullOrWhiteSpace(document.SourceFile)
                ? _store.GetSourceFile(document.Period) : document.SourceFile
        };
        state.Rows.AddRange(document.Rows.Select((row, index) => BillEditableRow.From(
            row.Number > 0 ? row : row with { Number = index + 1 })));
        state.View.SortDescriptions.Add(new SortDescription(nameof(BillEditableRow.SortNumber), ListSortDirection.Ascending));
        state.View.LiveSortingProperties.Add(nameof(BillEditableRow.SortNumber));
        state.View.IsLiveSorting = true;
        state.Footer = BillEditableFooter.From(document.Footer);
        void Changed(object? sender, PropertyChangedEventArgs args)
        {
            state.Dirty = true;
            if (sender is BillEditableRow row && args.PropertyName == nameof(BillEditableRow.ServiceName))
                UpdateServiceNames(new[] { row.ServiceName });
            Refresh();
        }
        var trackedRows = new HashSet<BillEditableRow>();
        void TrackRows()
        {
            foreach (var row in trackedRows.Where(row => !state.Rows.Contains(row)).ToArray())
            {
                row.PropertyChanged -= Changed;
                trackedRows.Remove(row);
            }
            foreach (var row in state.Rows)
                if (trackedRows.Add(row)) row.PropertyChanged += Changed;
        }
        TrackRows();
        state.Footer.PropertyChanged += Changed;
        state.Rows.CollectionChanged += (_, _) =>
        {
            TrackRows();
            state.Dirty = true;
            Refresh();
        };
        _states[document.Period] = state;
    }

    public void RefreshServiceNames() => UpdateServiceNames(Array.Empty<string>());

    private void UpdateServiceNames(IEnumerable<string> names)
    {
        var choices = names.Concat(ServiceNames)
            .Concat(_states.Values.SelectMany(state => state.Rows).Select(row => row.ServiceName))
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        foreach (var name in choices)
        {
            if (ServiceNames.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
            var index = 0;
            while (index < ServiceNames.Count && StringComparer.CurrentCultureIgnoreCase.Compare(ServiceNames[index], name) < 0)
                index++;
            ServiceNames.Insert(index, name);
        }
    }

    public async Task SaveParsedMonthsAsync(IEnumerable<ParsedBillRow> rows, IEnumerable<BillFooter> footers)
    {
        var rowGroups = rows.GroupBy(row => row.Period).ToDictionary(group => group.Key, group => group.ToArray());
        var summaries = footers.GroupBy(footer => footer.Period).ToDictionary(group => group.Key, group => group.First());
        IsBusy = true;
        var errors = new List<string>();
        try
        {
            foreach (var period in rowGroups.Keys.Concat(summaries.Keys).Distinct().OrderBy(period => period))
            {
                try
                {
                    BillMonthDocument document;
                    var existing = File.Exists(_store.GetPath(period)) ? await _store.LoadAsync(period) : null;
                    if (existing is not null) document = existing;
                    else
                    {
                        rowGroups.TryGetValue(period, out var monthRows);
                        summaries.TryGetValue(period, out var summary);
                        document = new(1, period, (monthRows ?? Array.Empty<ParsedBillRow>())
                            .Select((row, index) => row with { Number = index + 1 }).ToArray(), summary ?? new(period),
                            GetSourceFile(period, monthRows));
                        await _store.SaveAsync(document);
                    }
                    if (!_states.TryGetValue(period, out var state) || !state.Dirty) SetState(document);
                    if (!Periods.Contains(period)) Periods.Add(period);
                }
                catch (Exception exception) { errors.Add($"{period:yyyyMM}: {exception.Message}"); }
            }
            var sorted = Periods.OrderBy(period => period).ToArray();
            Periods.Clear();
            Periods.AddRange(sorted);
            UpdateServiceNames(rows.Select(row => row.ServiceName));
            SelectedRow = null;
            Status = errors.Count == 0 ? "Новые месяцы сохранены в JSON; существующие файлы не изменены." :
                "Не сохранены: " + string.Join("; ", errors);
        }
        finally { IsBusy = false; }
        if (SelectedPeriod is null && Periods.Count > 0) SelectedPeriod = Periods[0];
        Refresh(true);
        if (errors.Count > 0) throw new IOException(Status);
    }
    public void AddRow()
    {
        if (!CanAddRow) return;
        InsertRow(new BillEditableRow());
    }

    public void CopyRow()
    {
        if (!CanCopyRow || CommitPendingEdits?.Invoke() == false || !CanCopyRow) return;
        var source = SelectedRow!;
        InsertRow(new BillEditableRow
        {
            ServiceName = source.ServiceName,
            Unit = source.Unit,
            Volume = source.Volume,
            Tariff = source.Tariff,
            Charged = source.Charged,
            Recalculation = source.Recalculation,
            Total = source.Total
        });
    }

    private void InsertRow(BillEditableRow row)
    {
        var numberedRows = Rows!.Select(row => (Row: row, Number: int.TryParse(row.Number, out var number) ? number : 0))
            .Where(item => item.Number > 0).ToArray();
        var activeRow = SelectedRow is not null && Rows.Contains(SelectedRow) ? SelectedRow : null;
        if (activeRow is not null && (!int.TryParse(activeRow.Number, out var activeNumber) || activeNumber < 1))
        {
            Status = "Укажите корректный номер активной строки перед добавлением услуги.";
            return;
        }
        var lastNumber = numberedRows.Select(item => item.Number).DefaultIfEmpty(0).Max();
        if (lastNumber == int.MaxValue)
        {
            Status = "Нельзя добавить услугу: достигнут максимальный номер строки.";
            return;
        }
        var nextNumber = activeRow is null ? lastNumber + 1 : int.Parse(activeRow.Number);
        if (activeRow is not null)
            foreach (var item in numberedRows.Where(item => item.Number >= nextNumber).OrderByDescending(item => item.Number))
                item.Row.Number = (item.Number + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        row.Number = nextNumber.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Rows!.Add(row);
        SelectedRow = row;
    }
    public void DeleteRow()
    {
        if (!CanDeleteRow) return;
        var rows = Rows!;
        var orderedRows = rows.OrderBy(row => row.SortNumber).ToArray();
        var index = Array.IndexOf(orderedRows, SelectedRow);
        if (index < 0) return;
        var nextRow = index + 1 < orderedRows.Length ? orderedRows[index + 1] :
            index > 0 ? orderedRows[index - 1] : null;
        var deletedRow = orderedRows[index];
        rows.Remove(deletedRow);
        var number = 1;
        foreach (var row in orderedRows.Where(row => !ReferenceEquals(row, deletedRow)))
            row.Number = (number++).ToString(System.Globalization.CultureInfo.InvariantCulture);
        SelectedRow = nextRow;
    }

    private string GetSourceFile(DateTime period, IEnumerable<ParsedBillRow>? rows)
    {
        var fallback = _store.GetSourceFile(period);
        return rows?.Select(row => row.SourceFile).FirstOrDefault(file =>
            string.Equals(Path.GetExtension(file), Path.GetExtension(fallback), StringComparison.OrdinalIgnoreCase))
            ?? fallback;
    }

    private DateTime? FindPreviousPeriod() => Directory.Exists(_store.StorageDirectoryPath)
        ? Directory.EnumerateFiles(_store.StorageDirectoryPath, "*.json")
            .Select(path => DateTime.TryParseExact(Path.GetFileNameWithoutExtension(path), "yyyyMM",
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None,
                out var period) ? (DateTime?)period : null)
            .Where(period => period is not null && period < SelectedPeriod)
            .OrderByDescending(period => period).FirstOrDefault()
        : null;

    public void RepeatMonth()
    {
        if (!CanRepeatMonth) return;
        if (CommitPendingEdits?.Invoke() == false)
        {
            Status = "Завершите редактирование строки перед заменой услуг.";
            return;
        }
        try
        {
            var previousPeriod = FindPreviousPeriod();
            if (previousPeriod is null)
            {
                Status = "Более ранний месяц в JSON не найден.";
                return;
            }
            var replacement = _store.Load(previousPeriod.Value).Rows.OrderBy(row => row.Number)
                .Select((row, index) => new BillEditableRow
                {
                    Number = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ServiceName = row.ServiceName,
                    Unit = row.Unit,
                    Volume = BillEditableFields.Format(row.Volume),
                    Tariff = BillEditableFields.Format(row.Tariff)
                }).ToArray();
            if (MessageBox.Show($"Заменить все услуги текущего месяца услугами из {previousPeriod:yyyy-MM}?\n" +
                    "Начислено, перерасчет и итого в строках будут очищены. Итоги внизу останутся без изменений.",
                    "Повторить месяц", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            SelectedRow = null;
            Rows!.Clear();
            Rows.AddRange(replacement);
            RefreshServiceNames();
            SelectedRow = replacement.FirstOrDefault();
            Status = $"Услуги заменены из {previousPeriod:yyyy-MM}: {replacement.Length} строк. Изменения сохранятся автоматически.";
        }
        catch (Exception exception) { Status = $"Ошибка повторения месяца: {exception.Message}"; }
    }

    public void FillFromPrevious()
    {
        if (!CanFillFromPrevious) return;
        if (CommitPendingEdits?.Invoke() == false)
        {
            Status = "Завершите редактирование строки перед копированием из прошлого месяца.";
            return;
        }
        try
        {
            var previousPeriod = FindPreviousPeriod();
            if (previousPeriod is null)
            {
                Status = "Более ранний месяц в JSON не найден.";
                return;
            }
            var previousRows = _store.Load(previousPeriod.Value).Rows.OrderBy(row => row.Number)
                .Where(row => !string.IsNullOrWhiteSpace(row.ServiceName))
                .GroupBy(row => row.ServiceName.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => new Queue<ParsedBillRow>(group), StringComparer.OrdinalIgnoreCase);
            var copied = 0;
            foreach (var row in Rows!.OrderBy(row => row.SortNumber))
            {
                if (!previousRows.TryGetValue(row.ServiceName.Trim(), out var matches) || matches.Count == 0) continue;
                var source = matches.Dequeue();
                if (string.IsNullOrWhiteSpace(row.Unit) && !string.IsNullOrWhiteSpace(source.Unit))
                {
                    row.Unit = source.Unit;
                    copied++;
                }
                if (string.IsNullOrWhiteSpace(row.Volume) && source.Volume is { } volume)
                {
                    row.Volume = BillEditableFields.Format(volume);
                    copied++;
                }
                if (string.IsNullOrWhiteSpace(row.Tariff) && source.Tariff is { } tariff)
                {
                    row.Tariff = BillEditableFields.Format(tariff);
                    copied++;
                }
            }
            Status = $"Из месяца {previousPeriod:yyyy-MM} заполнено полей: {copied}.";
        }
        catch (Exception exception) { Status = $"Ошибка чтения прошлого месяца: {exception.Message}"; }
    }

    public void Recalculate()
    {
        if (!CanRecalculate) return;
        if (CommitPendingEdits?.Invoke() == false)
        {
            Status = "Завершите редактирование строки перед пересчетом.";
            return;
        }
        var changes = new List<(BillEditableRow Row, decimal? Charged, decimal? Total)>();
        foreach (var row in Rows!)
        {
            try
            {
                var volume = BillEditableFields.Number(row.Volume);
                var tariff = BillEditableFields.Number(row.Tariff);
                var charged = volume is { } quantity && tariff is { } rate
                    ? decimal.Round(checked(quantity * rate), 2, MidpointRounding.AwayFromZero)
                    : BillEditableFields.Number(row.Charged);
                var total = charged is { } amount
                    ? checked(amount + (BillEditableFields.Number(row.Recalculation) ?? 0))
                    : (decimal?)null;
                changes.Add((row, volume is not null && tariff is not null ? charged : null, total));
            }
            catch (Exception exception) when (exception is FormatException or OverflowException)
            {
                Status = $"Пересчет отменен. Строка {row.Number}: {exception.Message}";
                return;
            }
        }
        foreach (var change in changes)
        {
            if (change.Charged is { } charged) change.Row.Charged = BillEditableFields.Format(charged);
            if (change.Total is { } total) change.Row.Total = BillEditableFields.Format(total);
        }
        Status = "Начислено и итоги пересчитаны. Изменения сохранятся при смене месяца или закрытии формы.";
    }

    private async Task<bool> PersistMonth()
    {
        if (!CanPersistMonth) return false;
        var period = SelectedPeriod!.Value;
        IsBusy = true;
        try
        {
            await _store.SaveAsync(new(1, period, Rows!.OrderBy(row => row.SortNumber)
                .Select(row => row.ToRow(period)).ToArray(), Footer!.ToFooter(period), _states[period].SourceFile,
                _states[period].UtilityMeters, _states[period].Address, _states[period].AccountNumber));
            _states[period].Dirty = false;
            Status = $"Сохранено: {_store.GetPath(period)}";
            return true;
        }
        catch (Exception exception) { Status = $"Ошибка сохранения: {exception.Message}"; return false; }
        finally { IsBusy = false; }
    }
    public async Task LoadMonth()
    {
        if (!CanLoadMonth) return;
        var period = SelectedPeriod!.Value;
        if (_states[period].Dirty && MessageBox.Show("Заменить несохраненные правки данными из JSON?",
                "Загрузка месяца", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        IsBusy = true;
        try
        {
            SetState(await _store.LoadAsync(period));
            SelectedRow = null;
            UpdateServiceNames(Array.Empty<string>());
            Status = $"Загружено: {_store.GetPath(period)}";
        }
        catch (Exception exception) { Status = $"Ошибка загрузки: {exception.Message}"; }
        finally { IsBusy = false; Refresh(true); }
    }
    private void Refresh(bool changedMonth = false)
    {
        if (changedMonth)
        {
            NotifyOfPropertyChange(nameof(SelectedPeriod));
            NotifyOfPropertyChange(nameof(Rows));
            NotifyOfPropertyChange(nameof(SortedRows));
            NotifyOfPropertyChange(nameof(Footer));
            NotifyOfPropertyChange(nameof(UtilityMeters));
        }
        foreach (var name in new[]
            { nameof(CanEdit), nameof(CanSelectMonth), nameof(CanPreviousMonth),
            nameof(CanNextMonth), nameof(CanPersistMonth),
                     nameof(CanLoadMonth), nameof(CanAddRow), nameof(CanCopyRow),
            nameof(CanFillFromPrevious), nameof(CanRepeatMonth), nameof(CanRecalculate),
            nameof(CanDeleteRow),
                     nameof(PaymentReference),
                     nameof(IsReadOnly), nameof(ColumnTotal) })
            NotifyOfPropertyChange(name);
    }
    private sealed class MonthState
    {
        public MonthState()
        {
            View = new ListCollectionView(Rows);
        }
        public BindableCollection<BillEditableRow> Rows { get; } = new();
        public ListCollectionView View { get; }
        public BillEditableFooter Footer { get; set; } = new();
        public UtilityMeters UtilityMeters { get; set; } = new();
        public bool Dirty { get; set; }
        public string SourceFile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
