using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class RulesAndRatesViewModel(KeeperDataModel dataModel, IWindowManager windowManager) : Screen
{
    public string _title = null!;
    public DepoCondsModel Conditions { get; set; } = null!;
    public ObservableCollection<DepositRateLine> Rows { get; set; } = null!;
    public DateTime NewDate { get; set; } = DateTime.Today;

    public Visibility FormulaVisibility { get; set; }

    public List<string> Operations { get; set; } = new List<string>() { "*", "+", "/", "-" };

    private string _selectedOperation = null!;
    public string SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            if (value == _selectedOperation) return;
            _selectedOperation = value;
            Conditions.RateFormula = $"СР {SelectedOperation} {FormulaK}";
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(SaveFormulaAs));
        }
    }

    private double _formulaK;
    public double FormulaK
    {
        get => _formulaK;
        set
        {
            if (value.Equals(_formulaK)) return;
            _formulaK = value;
            Conditions.RateFormula = $"СР {SelectedOperation} {FormulaK}";
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(SaveFormulaAs));
        }
    }

    public string SaveFormulaAs => Conditions.RateFormula;

    public void Initialize(string title, DepoCondsModel conditions, RateType rateType)
    {
        _title = title;
        Conditions = conditions;

        FormulaVisibility = rateType == RateType.Linked ? Visibility.Visible : Visibility.Collapsed;
        if (rateType == RateType.Linked)
        {
            if (Conditions.RateFormula == null) Conditions.RateFormula = "СР * -1";
            RateFormula.TryParse(conditions.RateFormula, out string op, out double k);
            SelectedOperation = Operations.First(o => o == op);
            FormulaK = k;
        }

        Rows = [.. conditions.RateLines];
        if (Rows.Count == 0)
            Rows.Add(CreateRateLine(
                rateType == RateType.Linked ? (decimal)RateFormula.Calculate(Conditions.RateFormula, 1) : 0));
    }

    private static DepositRateLine CreateRateLine(decimal rate)
    {
        return new DepositRateLine()
        {
            DateFrom = DateTime.Today,
            AmountFrom = 0,
            AmountTo = 999999999999,
            Rate = rate,
        };
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _title;
    }

    public void AddLine()
    {
        var lastLine = Rows.Last();
        var newLine = new DepositRateLine()
        {
            DateFrom = lastLine.DateFrom,
            AmountFrom = lastLine.AmountTo + 0.01m,
            AmountTo = lastLine.AmountTo * 100 - 0.01m,
            Rate = lastLine.Rate,
        };
        Rows.Add(newLine);
    }

    public void RepeatDay()
    {
        var lastLine = Rows.Last();
        var copy = Rows.Where(r => r.DateFrom == lastLine.DateFrom)
            .Select(line => new DepositRateLine()
            {
                DateFrom = NewDate,
                AmountFrom = line.AmountFrom,
                AmountTo = line.AmountTo,
                Rate = line.Rate,
            })
            .ToList();

        foreach (var line in copy)
        {
            Rows.Add(line);
        }
    }

    // a) Button visible only if rate is LINKED
    // b) пока реализовано только для ставки связанной с Ставкой рефинансирования
    // c) чтобы не заводить новые строки с новыми Id - не удаляем существующие строки, а пересчитываем ставку
    public void RecalculateRates()
    {
        UpdateTable();
    }

    // эта функция нужна только если введены неправильные ставки
    public void RecalculateExistingLines()
    {
        var table = Rows.ToList();
        Rows.Clear();

        foreach (var depositRateLine in table)
        {
            var l = dataModel.RefinancingRates.Last(r => r.Date.Date <= depositRateLine.DateFrom.Date);
            depositRateLine.Rate = (decimal)RateFormula.Calculate(Conditions.RateFormula, l.Value);
            Rows.Add(depositRateLine);
        }
    }

    // таблицы ставок должны обновляться централизовано, после ввода новой ставки рефинансирования
    // единственный случай когда нужна кнопка здесь - когда заводим новый депозит/новые условия
    private void UpdateTable()
    {
        dataModel.UpdateRateLinesInConditions(Conditions);
        Rows.Clear();
        Conditions.RateLines.ForEach(Rows.Add);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        if (Rows.Count == 0)
        {
            var vm = new MyMessageBoxViewModel(MessageType.Error, "Таблица не должна быть пустая");
            await windowManager.ShowDialogAsync(vm);
            return await base.CanCloseAsync(cancellationToken);
        }

        Conditions.RateLines = [.. Rows];
        return await base.CanCloseAsync(cancellationToken);
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
