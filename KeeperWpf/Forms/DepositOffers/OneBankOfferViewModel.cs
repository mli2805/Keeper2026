using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

[ExportViewModel]
public class OneBankOfferViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
    RulesAndRatesViewModel rulesAndRatesViewModel) : Screen
{
    private readonly string _dateTemplate = "dd-MM-yyyy";

    public List<AccountItemModel> Banks { get; set; } = null!;
    public List<string> BankNames { get; set; } = null!;
    public string SelectedBankName { get; set; } = null!;

    public List<CurrencyCode> Currencies { get; set; } = null!;
    public List<RateType> RateTypes { get; set; } = null!;
    public List<Durations> Durations { get; set; } = null!;
    public DepositOfferModel ModelInWork { get; set; } = null!;

    public List<string> ConditionDates { get; set; } = null!;


    public string SelectedDate { get; set; } = null!;

    private IEnumerable<DepoCondsModel> OrderedConditions => ModelInWork.CondsList.OrderBy(c => c.DateFrom);

    public bool IsCancelled { get; set; }

    public void Initialize(DepositOfferModel model)
    {
        Banks = keeperDataModel.AcMoDict[220].Children.Select(c=>(AccountItemModel)c).ToList();
        BankNames = Banks.Select(b => b.Name).ToList();
        SelectedBankName = BankNames.First(n => n == model.Bank.Name);
        Currencies = Enum.GetValues<CurrencyCode>().ToList();
        RateTypes = Enum.GetValues<RateType>().ToList();
        Durations = Enum.GetValues<Durations>().ToList();
        ModelInWork = model;
        ConditionDates = OrderedConditions.Select(c => c.DateFrom.ToString(_dateTemplate)).ToList();
        if (ConditionDates.Count > 0) SelectedDate = ConditionDates.Last();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Банковский депозит";
    }

    public async Task AddConditions()
    {
        var date = DateTime.Today;
        while (ModelInWork.CondsList.Any(c => c.DateFrom == date)) date = date.AddDays(1);

        var depoCondsModel = new DepoCondsModel()
        {
            DateFrom = date,
        };
        rulesAndRatesViewModel.Initialize(ModelInWork.Title, depoCondsModel, ModelInWork.RateType);
        await windowManager.ShowDialogAsync(rulesAndRatesViewModel);
        ModelInWork.CondsList.Add(depoCondsModel);
        ConditionDates = OrderedConditions.Select(c => c.DateFrom.ToString(_dateTemplate)).ToList();
        SelectedDate = depoCondsModel.DateFrom.ToString(_dateTemplate);
        NotifyOfPropertyChange(nameof(ConditionDates));
        NotifyOfPropertyChange(nameof(SelectedDate));
    }

    public async Task EditConditions()
    {
        if (SelectedDate == null) return;
        var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());
        var condition = ModelInWork.CondsList.First(c => c.DateFrom == date);
        rulesAndRatesViewModel.Initialize(ModelInWork.Title, condition, ModelInWork.RateType);
        await windowManager.ShowDialogAsync(rulesAndRatesViewModel);
        if (date == condition.DateFrom) return;
        if (ModelInWork.CondsList.Any(c => c != condition && c.DateFrom == condition.DateFrom))
        {
            condition.DateFrom = date;
            return;
        }

        ConditionDates = OrderedConditions.Select(c => c.DateFrom.ToString(_dateTemplate)).ToList();
        SelectedDate = condition.DateFrom.ToString(_dateTemplate);
        NotifyOfPropertyChange(nameof(ConditionDates));
        NotifyOfPropertyChange(nameof(SelectedDate));
    }

    public void RemoveConditions()
    {
        if (SelectedDate == null) return;
        var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());

        var condition = ModelInWork.CondsList.FirstOrDefault(c => c.DateFrom == date);
        if (condition == null) return;

        ModelInWork.CondsList.Remove(condition);

        ConditionDates = OrderedConditions.Select(c => c.DateFrom.ToString(_dateTemplate)).ToList();
        SelectedDate = ConditionDates.LastOrDefault()!;
        NotifyOfPropertyChange(nameof(ConditionDates));
        NotifyOfPropertyChange(nameof(SelectedDate));
    }

    public async Task Save()
    {
        if (ModelInWork.CondsList.Count == 0)
        {
            await AddConditions();
            return;
        }
        ModelInWork.Bank = Banks.First(b => b.Name == SelectedBankName);
        IsCancelled = false;
        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        IsCancelled = true;
        await TryCloseAsync();
    }

}
