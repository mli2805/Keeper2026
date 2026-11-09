using Caliburn.Micro;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;

namespace KeeperWpf;

[ExportViewModel]
public class MoneyBackViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
    MccReferenceBookViewModel mccReferenceBookViewModel,
    OneMoneyBackRulesViewModel oneMoneyBackRulesViewModel) : Screen
{
    public OneMoneyBackRulesViewModel RulesViewModel { get; } = oneMoneyBackRulesViewModel;

    private DepositOfferModel _depositOfferInWork { get; set; } = null!;
    public string CardName { get; set; } = null!;
    private MoneyBackRulesModel? _moneyBackRulesInWork;
    public MoneyBackRulesModel? MoneyBackRulesInWork
    {
        get => _moneyBackRulesInWork;
        set
        {
            _moneyBackRulesInWork = value;
            NotifyOfPropertyChange(nameof(MoneyBackRulesInWork));
            RulesViewModel.ApplyModel(value);
        }
    }
    public ObservableCollection<MoneyBackRulesModel> ListOfMoneyBackPrograms { get; set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Правила начисления манибэка по {CardName}";
    }

    public void Initialize(DepositOfferModel card)
    {
        _depositOfferInWork = card;
        CardName = card.ToString();
        ListOfMoneyBackPrograms = new ObservableCollection<MoneyBackRulesModel>(card.MoneyBackRules);
        MoneyBackRulesInWork = card.MoneyBackRules.LastOrDefault();
    }

    public async Task ShowMccReferenceBook()
    {
        await windowManager.ShowWindowAsync(mccReferenceBookViewModel);
    }

    public void AddNewSetOfRules()
    {
        // Implementation for adding a new set of rules
        var newRules = new MoneyBackRulesModel
        {
            DepositOfferId = _depositOfferInWork.Id,
            StartDate = DateTime.Today,
            MoneyBackType = MoneyBackType.AnyWithExemptions,
            Percent = 0.0,

            MinPurchaseAmount = 10,
            MaxPurchaseAmount = null,
            MaxMoneyBackAmount = 100,
            MinMonthPurchasesAmount = null,
        };
        ListOfMoneyBackPrograms.Add(newRules);
        MoneyBackRulesInWork = newRules;
    }

    public void RemoveSetOfRules()
    {
        // Implementation for removing a set of rules
    }
}
