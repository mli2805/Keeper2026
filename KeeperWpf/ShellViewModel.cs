using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeeperWpf;

public class ShellViewModel : Screen, IShell
{
    private readonly IWindowManager _windowManager;
    private readonly KeeperDataModel _keeperDataModel;
    private readonly KeeperDataModelInitializer _dataModelInitializer;
    private readonly LoadingProgressViewModel _loadingProgressViewModel;


    public ShellPartsBinder ShellPartsBinder { get; }
    public MainMenuViewModel MainMenuViewModel { get; }
    public AccountTreeViewModel AccountTreeViewModel { get; }
    public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; }

    public ShellViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel,
        KeeperDataModelInitializer dataModelInitializer, LoadingProgressViewModel loadingProgressViewModel,
        ShellPartsBinder shellPartsBinder, MainMenuViewModel mainMenuViewModel,
        AccountTreeViewModel accountTreeViewModel, BalanceOrTrafficViewModel balanceOrTrafficViewModel,
        TwoSelectorsViewModel twoSelectorsViewModel)
    {
        _windowManager = windowManager;
        _keeperDataModel = keeperDataModel;
        _dataModelInitializer = dataModelInitializer;
        _loadingProgressViewModel = loadingProgressViewModel;
        ShellPartsBinder = shellPartsBinder;
        MainMenuViewModel = mainMenuViewModel;
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
       
        var account = _keeperDataModel.AccountsTree.First(r => r.Name == "Мои");
        account.IsSelected = true;
        MainMenuViewModel.SetBellPath();
        ShellPartsBinder.SelectedAccountItemModel = account;
    }

    public async Task<bool> LoadAccountsTree()
    {
        // если БД удалили, она будет создана пустая в AppBootstrapper еще до ShellViewModel
        // GetAccountTreeFromDb вернет false, если в БД нет данных
        if (await _dataModelInitializer.GetFullModelFromDb())
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

        var success = await _dataModelInitializer.GetFullModelFromDb();
        return true;
    }
}