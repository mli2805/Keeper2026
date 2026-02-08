using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

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

    public bool IsCancelled { get; set; }

    public void Initialize(DepositOfferModel model)
    {
        Banks = keeperDataModel.AcMoDict[220].Children.Select(c=>(AccountItemModel)c).ToList();
        BankNames = Banks.Select(b => b.Name).ToList();
        SelectedBankName = BankNames.First(n => n == model.Bank.Name);
        Currencies = Enum.GetValues<CurrencyCode>().OfType<CurrencyCode>().ToList();
        RateTypes = Enum.GetValues<RateType>().OfType<RateType>().ToList();
        Durations = Enum.GetValues<Durations>().OfType<Durations>().ToList();
        ModelInWork = model;
        ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
        if (ConditionDates.Count > 0) SelectedDate = ConditionDates.Last();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Банковский депозит";
    }

    public async Task AddConditions()
    {
        var date = DateTime.Today;
        while (ModelInWork.CondsMap.ContainsKey(date)) date = date.AddDays(1);

        //var lastIdInDb = _keeperDataModel.GetDepoConditionsMaxId();
        //var lastIdHere = ModelInWork.CondsMap.Any()
        //    ? ModelInWork.CondsMap.Values.ToList().Max(c => c.Id)
        //    : 0;
        //var maxId = Math.Max(lastIdInDb, lastIdHere);

        var depoCondsModel = new DepoCondsModel()
        {
            //DepositOfferId = ModelInWork.Id,
            DateFrom = date,
        };
        rulesAndRatesViewModel.Initialize(ModelInWork.Title, depoCondsModel, ModelInWork.RateType);
        await windowManager.ShowDialogAsync(rulesAndRatesViewModel);
        ModelInWork.CondsMap.Add(depoCondsModel.DateFrom, depoCondsModel);
        ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
        NotifyOfPropertyChange(nameof(ConditionDates));
    }

    //private int GetMaxDepoRateLineId()
    //{
    //    var lastInDb = _keeperDataModel.GetDepoRateLinesMaxId();
    //    var lastIdHere = ModelInWork.CondsMap.Any()
    //        ? ModelInWork.CondsMap.Values.ToList()
    //            .SelectMany(c => c.RateLines).Max(r => r.Id)
    //        : 0;
    //    return Math.Max(lastInDb, lastIdHere);
    //}

    public async Task EditConditions()
    {
        if (SelectedDate == null) return;
        var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());
        rulesAndRatesViewModel.Initialize(ModelInWork.Title, ModelInWork.CondsMap[date], ModelInWork.RateType);
        await windowManager.ShowDialogAsync(rulesAndRatesViewModel);
        if (date == ModelInWork.CondsMap[date].DateFrom) return;

        var depoCondsModel = ModelInWork.CondsMap[date];
        ModelInWork.CondsMap.Remove(date);
        ModelInWork.CondsMap.Add(depoCondsModel.DateFrom, depoCondsModel);

        ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
        NotifyOfPropertyChange(nameof(ConditionDates));
    }

    public void RemoveConditions()
    {
        if (SelectedDate == null) return;
        var date = DateTime.ParseExact(SelectedDate, _dateTemplate, new DateTimeFormatInfo());

        ModelInWork.CondsMap.Remove(date);

        ConditionDates = ModelInWork.CondsMap.Keys.Select(d => d.ToString(_dateTemplate)).ToList();
        NotifyOfPropertyChange(nameof(ConditionDates));
    }

    public async Task Save()
    {
        if (ModelInWork.CondsMap.Count == 0)
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
