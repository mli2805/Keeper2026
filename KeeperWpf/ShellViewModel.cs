using Caliburn.Micro;
using KeeperInfrastructure;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class ShellViewModel : Screen, IShell
{
    private readonly IWindowManager _windowManager;
    private readonly KeeperDataModel _keeperDataModel;
    private readonly ShellPartsBinder _shellPartsBinder;
    private readonly KeeperDataModelInitializer _dataModelInitializer;
    private readonly LoadingProgressViewModel _loadingProgressViewModel;
    private readonly RatesViewModel _ratesViewModel;


    public AccountTreeViewModel AccountTreeViewModel { get; }
    public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; }

    public ShellViewModel(IWindowManager windowManager,
        KeeperDataModel keeperDataModel, ShellPartsBinder shellPartsBinder,
        KeeperDataModelInitializer dataModelInitializer, LoadingProgressViewModel loadingProgressViewModel,
        RatesViewModel ratesViewModel, 
        AccountTreeViewModel accountTreeViewModel, BalanceOrTrafficViewModel balanceOrTrafficViewModel,
        TwoSelectorsViewModel twoSelectorsViewModel)
    {
        _windowManager = windowManager;
        _keeperDataModel = keeperDataModel;
        _shellPartsBinder = shellPartsBinder;
        _dataModelInitializer = dataModelInitializer;
        _loadingProgressViewModel = loadingProgressViewModel;
        _ratesViewModel = ratesViewModel;
        AccountTreeViewModel = accountTreeViewModel;
        BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
        TwoSelectorsViewModel = twoSelectorsViewModel;
    }

    protected override async void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";
        var success = await LoadAccountsTree();
        if (!success)
        {
            await TryCloseAsync();
            return;
        }

        // нужны транзакции для расчета остатков на счетах
        _dataModelInitializer.GetTransactionsFromDb();
        // и курсы для отображения остатков в разных валютах
        _dataModelInitializer.GetOfficialRatesFromDb(); 
        _dataModelInitializer.GetExchangeRatesFromDb(); 
        _dataModelInitializer.GetMetalRatesFromDb();

        var account = _keeperDataModel.AccountsTree.First(r => r.Name == "Мои");
        account.IsSelected = true;
        _shellPartsBinder.SelectedAccountItemModel = account;
    }

    public async Task<bool> LoadAccountsTree()
    {
        // если БД удалили, она будет создана в AppBootstrapper еще до ShellViewModel
        // GetAccountTreeFromDb вернет false, если в БД нет данных
        if (await _dataModelInitializer.GetAccountTreeAndDictionaryFromDb())
        {
            return true;    
        }

        var mb = new MyMessageBoxViewModel(MessageType.Confirmation,
            new List<string>()
            {
                "База данных пуста!", "Загрузить данные из текстовых файлов?"
            });
        var confirmation = await _windowManager.ShowDialogAsync(mb);
        if (confirmation == null || confirmation.Value == false)
        {
            return false;
        }

        var success2 = await _windowManager.ShowDialogAsync(_loadingProgressViewModel);
        if (success2 == null || success2.Value == false)
        {
            return false;
        }

        var success = await _dataModelInitializer.GetAccountTreeAndDictionaryFromDb();
        return true;
    }

    public async void ShowRatesForm()
    {
        await _ratesViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_ratesViewModel);
    }

    public async void ShowCarsForm()
    {
        // 
    }
}