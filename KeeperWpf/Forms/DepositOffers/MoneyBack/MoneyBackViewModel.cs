using Caliburn.Micro;
using KeeperInfrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class MoneyBackViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
    DepositOffersRepository depositOffersRepository,
    MccReferenceBookViewModel mccReferenceBookViewModel,
    OneMoneyBackRulesViewModel oneMoneyBackRulesViewModel) : Screen
{
    public OneMoneyBackRulesViewModel RulesViewModel { get; } = oneMoneyBackRulesViewModel;

    private DepositOfferModel _depositOfferInWork { get; set; } = null!;
    public string CardName { get; set; } = null!;

    public ObservableCollection<MoneyBackRulesModel> Rows { get; set; }
        = new ObservableCollection<MoneyBackRulesModel>();
    private MoneyBackRulesModel _selectedSetOfRules = null!;
    public MoneyBackRulesModel SelectedSetOfRules
    {
        get => _selectedSetOfRules;
        set
        {
            _selectedSetOfRules = value;
            NotifyOfPropertyChange(nameof(SelectedSetOfRules));
            RulesViewModel.ApplyModel(value);
        }
    }
    

    protected override void OnViewLoaded(object view)
    {
        DisplayName = $"Правила начисления манибэка по {CardName}";
    }

    public void Initialize(DepositOfferModel card)
    {
        RulesViewModel.Parent = this; // чтобы сработал CanCloseAsync вложенной формы
        _depositOfferInWork = card;
        CardName = card.ToString();
        if (card.MoneyBackRules.Count == 0)
        {
            Rows.Add(Create());
        }
        else
        {
            foreach (var setOfRules in card.MoneyBackRules)
            {
                Rows.Clear();
                Rows.Add(setOfRules);
            }
        }
        SelectedSetOfRules = Rows.Last();
    }

    public async Task ShowMccReferenceBook()
    {
        await windowManager.ShowWindowAsync(mccReferenceBookViewModel);
    }

    public void AddNewSetOfRules()
    {
        Rows.Add(Create());
        SelectedSetOfRules = Rows.Last();
    }

    private MoneyBackRulesModel Create()
    {
        return new MoneyBackRulesModel
        {
            DepositOfferId = _depositOfferInWork.Id,
            StartDate = DateTime.Today,
            MoneyBackType = MoneyBackType.AnyWithExemptions,
            Percent = 2.0,

            MinPurchaseAmount = 10,
            MaxPurchaseAmount = null,
            MaxMoneyBackAmount = 100,
            MinMonthPurchasesAmount = null,
        };
    }

    public void RemoveSetOfRules()
    {
        // Implementation for removing a set of rules

        // Если не осталось ни одного набора правил, создаем 1 по умолчанию, чтобы не ломать логику формы
        // В базе при этом могут быть карточки без правил манибэка
        if (Rows.Count == 0)
        {
            Rows.Add(Create());
            SelectedSetOfRules = Rows.Last();
        }
    }

    public async Task Save()
    {
        RulesViewModel.ApplyControlChangesToModel(); // переносим правки гридов в текущую модель
        _depositOfferInWork.MoneyBackRules = Rows.ToList();
        var dataModelOffer = dataModel.DepositOffers.FirstOrDefault(o => o.Id == _depositOfferInWork.Id);
        if (dataModelOffer != null && !ReferenceEquals(dataModelOffer, _depositOfferInWork))
            dataModelOffer.MoneyBackRules = _depositOfferInWork.MoneyBackRules;

        await depositOffersRepository.SaveMoneyBackRules(_depositOfferInWork.Id, _depositOfferInWork.MoneyBackRules);
        await TryCloseAsync();
    }

    public async Task Cancel()
    {
        await TryCloseAsync();
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return await base.CanCloseAsync(cancellationToken);
    }
}
