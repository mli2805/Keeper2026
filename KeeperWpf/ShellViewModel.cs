using Caliburn.Micro;
using KeeperInfrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

public class ShellViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel, AccountRepository accountRepository,
    KeeperDataModelInitializer dataModelInitializer, LoadingProgressViewModel loadingProgressViewModel,
    ShellPartsBinder shellPartsBinder, MainMenuViewModel mainMenuViewModel,
    AccountTreeViewModel accountTreeViewModel, BalanceOrTrafficViewModel balanceOrTrafficViewModel,
    TwoSelectorsViewModel twoSelectorsViewModel) : Screen, IShell
{


    public ShellPartsBinder ShellPartsBinder { get; } = shellPartsBinder;
    public MainMenuViewModel MainMenuViewModel { get; } = mainMenuViewModel;
    public AccountTreeViewModel AccountTreeViewModel { get; } = accountTreeViewModel;
    public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; } = balanceOrTrafficViewModel;
    public TwoSelectorsViewModel TwoSelectorsViewModel { get; } = twoSelectorsViewModel;

    protected override async void OnViewLoaded(object view)
    {
        DisplayName = "Keeper 2026";
        var success = await LoadAccountsTree();
        if (!success)
        {
            await TryCloseAsync();
            return;
        }
       
        var account = keeperDataModel.AccountsTree.First(r => r.Name == "Мои");
        account.IsSelected = true;
        MainMenuViewModel.SetBellPath();
        ShellPartsBinder.SelectedAccountItemModel = account;
    }

    public async Task<bool> LoadAccountsTree()
    {
        // если БД удалили, она будет создана пустая в AppBootstrapper еще до ShellViewModel
        // GetAccountTreeFromDb вернет false, если в БД нет данных
        if (await dataModelInitializer.GetFullModelFromDb())
        {
            return true;
        }

        var mb = new MyMessageBoxViewModel(MessageType.Confirmation,
            new List<string>()
            {
                "База данных пуста!", "Загрузить данные из текстовых файлов?"
            });
        var confirmation = await windowManager.ShowDialogAsync(mb);
        if (confirmation == null || confirmation.Value == false)
        {
            return false;
        }

        var success2 = await windowManager.ShowDialogAsync(loadingProgressViewModel);
        if (success2 == null || success2.Value == false)
        {
            return false;
        }

        var success = await dataModelInitializer.GetFullModelFromDb();
        return true;
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await accountRepository.UpdateTree(keeperDataModel.FlattenAccountTree());
        await MainMenuViewModel.SaveInTextFilesForBackup();
        return await base.CanCloseAsync(cancellationToken);
    }
}