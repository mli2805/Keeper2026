using Caliburn.Micro;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KeeperWpf;

[ExportViewModel]
public class MoneyBackViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
    MccReferenceBookViewModel mccReferenceBookViewModel) : Screen
{
    private DepositOfferModel _cardInWork { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public MoneyBackRulesModel? MoneyBackRulesInWork { get; set; }
    public List<MoneyBackRulesModel> ListOfMoneyBackPrograms { get; set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Правила начисления манибэка по {CardName}";
    }

    public void Initialize(DepositOfferModel card)
    {
        _cardInWork = card;
        CardName = card.ToString();
        ListOfMoneyBackPrograms = card.MoneyBackRules;
        MoneyBackRulesInWork = card.MoneyBackRules.LastOrDefault();
    }

    public async Task ShowMccReferenceBook()
    {
        await windowManager.ShowWindowAsync(mccReferenceBookViewModel);
    }

    public void AddNewSetOfRules()
    {
        // Implementation for adding a new set of rules
    }

    public void RemoveSetOfRules()
    {
        // Implementation for removing a set of rules
    }
}
