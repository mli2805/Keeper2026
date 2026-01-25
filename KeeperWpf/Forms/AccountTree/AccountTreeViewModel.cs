using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;

namespace KeeperWpf;

public class AccountTreeViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
    AskDragAccountActionViewModel askDragAccountActionViewModel, AccountRepository accountRepository,
    OneFolderViewModel oneFolderViewModel, OneAccountViewModel oneAccountViewModel, OneBankAccountViewModel oneBankAccountViewModel,
    ExpensesOnAccountViewModel expensesOnAccountViewModel,
    DepositInterestViewModel depositInterestViewModel, CardFeeViewModel cardFeeViewModel,
    DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel,
    PaymentWaysViewModel paymentWaysViewModel) : PropertyChangedBase
{

    public IWindowManager WindowManager { get; } = windowManager;
    public ShellPartsBinder ShellPartsBinder { get; } = shellPartsBinder;
    public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; } = askDragAccountActionViewModel;

    public KeeperDataModel KeeperDataModel { get; set; } = keeperDataModel;

    public async Task AddFolder()
    {
        var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
            "", ShellPartsBinder.SelectedAccountItemModel);
        accountItemModel.IsFolder = true;

        oneFolderViewModel.Initialize(accountItemModel, true);
        await WindowManager.ShowDialogAsync(oneFolderViewModel);
        if (!oneFolderViewModel.IsSavePressed) return;

        ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
        KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
    }

    public async Task AddAccount(int param)
    {
        var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
                "", ShellPartsBinder.SelectedAccountItemModel);

        // if ((accountItemModel.Parent?.Parent?.Id ?? 0) == 161)
        if (param == 1) // контрагент или тэг
        {
            oneAccountViewModel.Initialize(accountItemModel, true);
            await WindowManager.ShowDialogAsync(oneAccountViewModel);
            if (!oneAccountViewModel.IsSavePressed) return;
        }
        else // 2 - счет в банке; 3 - депозит; 4 - карточка 
        {
            accountItemModel.BankAccount = new BankAccountModel { Id = accountItemModel.Id, };
            if (param == 3)
                accountItemModel.BankAccount.Deposit = new Deposit() { Id = accountItemModel.Id };
            if (param == 4)
                accountItemModel.BankAccount.PayCard = new PayCard() { Id = accountItemModel.Id };

            oneBankAccountViewModel.Initialize(accountItemModel, true, param == 4);
            await WindowManager.ShowDialogAsync(oneBankAccountViewModel);
            if (!oneBankAccountViewModel.IsSavePressed) return;
        }

        ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
        KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
    }

    public async Task ChangeAccount()
    {
        var accountItemModel = ShellPartsBinder.SelectedAccountItemModel;
        if (accountItemModel.IsFolder)
        {
            oneFolderViewModel.Initialize(accountItemModel, false);
            await WindowManager.ShowDialogAsync(oneFolderViewModel);
        }
        else if (accountItemModel.IsCard)
        {
            oneBankAccountViewModel.Initialize(accountItemModel, false, true);
            await WindowManager.ShowDialogAsync(oneBankAccountViewModel);
        }
        else if (accountItemModel.IsDeposit)
        {
            oneBankAccountViewModel.Initialize(accountItemModel, false, false);
            await WindowManager.ShowDialogAsync(oneBankAccountViewModel);
        }
        else if (accountItemModel.IsBankAccount)
        {
            oneBankAccountViewModel.Initialize(accountItemModel, false, false);
            await WindowManager.ShowDialogAsync(oneBankAccountViewModel);
        }
        else
        {
            oneAccountViewModel.Initialize(accountItemModel, false);
            await WindowManager.ShowDialogAsync(oneAccountViewModel);
        }
    }

    public async void RemoveSelectedAccount()
    {
        var removedId = await KeeperDataModel.RemoveSelectedAccount();
        if (removedId < 0) return;
        await accountRepository.DeleteByAccountId(removedId);
    }

    // дергаем из код бехайнда дерева при окончании DragAndDrop
    public async Task SaveDragAndDrops()
    {
        var accounts = KeeperDataModel.FlattenAccountTree();
        await accountRepository.UpdateTree(accounts);
    }

    public async Task EnrollDepositInterest()
    {
        depositInterestViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(depositInterestViewModel);
    }

    public async Task TakeCardFee()
    {
        cardFeeViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(cardFeeViewModel);
    }

    public async Task ShowDepositReport()
    {
        depositReportViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(depositReportViewModel);
    }

    public async Task ShowVerificationForm()
    {
        balanceVerificationViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(balanceVerificationViewModel);
    }

    public async Task ShowFolderSummaryForm()
    {
        var folderSummaryViewModel = new FolderSummaryViewModel(KeeperDataModel);
        folderSummaryViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowWindowAsync(folderSummaryViewModel);
    }

    public async Task ShowPaymentWaysForm()
    {
        paymentWaysViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowWindowAsync(paymentWaysViewModel);
    }

    public async Task ShowExpensesOnAccount()
    {
        expensesOnAccountViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod);
        await WindowManager.ShowDialogAsync(expensesOnAccountViewModel);
    }
}
