using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;

namespace KeeperWpf;

public class BankOffersViewModel(IWindowManager windowManager, KeeperDataModel dataModel,
    DepositOffersRepository depositOffersRepository, OneBankOfferViewModel oneBankOfferViewModel) : Screen
{
    public ObservableCollection<DepositOfferModel> Rows { get; set; } = null!;

    private DepositOfferModel _selectedDepositOffer = null!;
    public DepositOfferModel SelectedDepositOffer
    {
        get => _selectedDepositOffer;
        set
        {
            if (Equals(value, _selectedDepositOffer)) return;
            _selectedDepositOffer = value;
            NotifyOfPropertyChange();
        }
    }

    // 161 - папка Карточки
    // 166 - папка Депозиты
    // 902 - папка Трастовые

    public SolidColorBrush DepositBrush { get; set; } = Brushes.PaleGreen;
    public SolidColorBrush CardBrush { get; set; } = Brushes.PeachPuff;
    public SolidColorBrush TrustBrush { get; set; } = Brushes.LightPink;
    public SolidColorBrush ClosedBrush { get; set; } = Brushes.LightGray;
    public SolidColorBrush NotInUseBrush { get; set; } = Brushes.Transparent;
    public void Initialize()
    {
        Rows = [];

        foreach (var depositOfferModel in dataModel.DepositOffers)
        {
            var account = dataModel.AcMoDict.Values.FirstOrDefault(a =>
                (a.IsDeposit && a.BankAccount!.DepositOfferId == depositOfferModel.Id)
                || (a.IsCard && a.BankAccount!.DepositOfferId == depositOfferModel.Id));

            if (account != null)
            {
                if (account.Is(166))
                    depositOfferModel.BackgroundColor = DepositBrush;
                else if (account.Is(161))
                    depositOfferModel.BackgroundColor = CardBrush;
                else if (account.Is(902))
                    depositOfferModel.BackgroundColor = TrustBrush;
                else // счет к такой оферте существует, но не в этих папках, значит закрыт 
                    depositOfferModel.BackgroundColor = ClosedBrush;
            }
            else
            {
                // для этой оферты нет счета вообще (счет открыт не как депозит), желательно переделать каким-то образом
                depositOfferModel.BackgroundColor = NotInUseBrush;
            }
            Rows.Add(depositOfferModel);
        }

        SelectedDepositOffer = Rows.Last();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Банковские депозиты";
    }

    public async Task AddOffer()
    {
        var offerModel = new DepositOfferModel
        {
            Bank = SelectedDepositOffer.Bank,
            MainCurrency = CurrencyCode.BYN,
            DepositTerm = new DurationModel(1, Durations.Years),
        };

        oneBankOfferViewModel.Initialize(offerModel);
        await windowManager.ShowDialogAsync(oneBankOfferViewModel);
        if (oneBankOfferViewModel.IsCancelled) return;

        var newDepositOffer = await depositOffersRepository
            .AddDepositOffer(oneBankOfferViewModel.ModelInWork, dataModel.AcMoDict);

        Rows.Add(newDepositOffer);
        dataModel.DepositOffers.Add(newDepositOffer);
        SelectedDepositOffer = newDepositOffer;
    }

    public async Task EditSelectedOffer()
    {
        var offerModel = SelectedDepositOffer.DeepCopyExceptBank();
        oneBankOfferViewModel.Initialize(offerModel);
        await windowManager.ShowDialogAsync(oneBankOfferViewModel);
        if (oneBankOfferViewModel.IsCancelled) return;

        var updatedDepositOffer = await depositOffersRepository
            .UpdateDepositOffer(oneBankOfferViewModel.ModelInWork, dataModel.AcMoDict);

        var index = Rows.IndexOf(SelectedDepositOffer);
        Rows.Remove(SelectedDepositOffer);
        Rows.Insert(index, updatedDepositOffer);
        dataModel.DepositOffers = [.. Rows];
        SelectedDepositOffer = updatedDepositOffer;
    }

    public async Task RemoveSelectedOffer()
    {
        if (dataModel.AcMoDict.Values.Any(a => a.IsDeposit && a.BankAccount!.DepositOfferId == SelectedDepositOffer.Id))
        {
            var strs = new List<string> { "Существует как минимум один депозит открытый по этой оферте.", "", "Сначала удалите депозиты." };
            var vm = new MyMessageBoxViewModel(MessageType.Error, strs);
            await windowManager.ShowDialogAsync(vm);
            return;
        }
        await depositOffersRepository.DeleteDepositOffer(SelectedDepositOffer.Id);
        Rows.Remove(SelectedDepositOffer);
        dataModel.DepositOffers = [.. Rows];
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        dataModel.DepositOffers = [.. Rows];
        return await base.CanCloseAsync(cancellationToken);
    }
}
