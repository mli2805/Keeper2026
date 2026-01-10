using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;

namespace KeeperWpf;

public class AccountTreeViewModel : PropertyChangedBase
{
    private readonly OneAccountViewModel _oneAccountViewModel;
    private readonly OneBankAccountViewModel _oneBankAccountViewModel;
    private readonly ExpensesOnAccountViewModel _expensesOnAccountViewModel;
    private readonly DepositInterestViewModel _depositInterestViewModel;
    private readonly CardFeeViewModel _cardFeeViewModel;
    private readonly DepositReportViewModel _depositReportViewModel;
    private readonly BalanceVerificationViewModel _balanceVerificationViewModel;
    private readonly PaymentWaysViewModel _paymentWaysViewModel;

    public IWindowManager WindowManager { get; }
    public ShellPartsBinder ShellPartsBinder { get; }
    public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

    public KeeperDataModel KeeperDataModel { get; set; }


    public AccountTreeViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
        AskDragAccountActionViewModel askDragAccountActionViewModel,
        OneAccountViewModel oneAccountViewModel, OneBankAccountViewModel oneBankAccountViewModel,
        ExpensesOnAccountViewModel expensesOnAccountViewModel,
        DepositInterestViewModel depositInterestViewModel, CardFeeViewModel cardFeeViewModel,
        DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel,
        PaymentWaysViewModel paymentWaysViewModel)
    {
        _oneAccountViewModel = oneAccountViewModel;
        _oneBankAccountViewModel = oneBankAccountViewModel;
        _expensesOnAccountViewModel = expensesOnAccountViewModel;
        _depositInterestViewModel = depositInterestViewModel;
        _cardFeeViewModel = cardFeeViewModel;
        _depositReportViewModel = depositReportViewModel;
        _balanceVerificationViewModel = balanceVerificationViewModel;
        _paymentWaysViewModel = paymentWaysViewModel;
        WindowManager = windowManager;
        ShellPartsBinder = shellPartsBinder;
        AskDragAccountActionViewModel = askDragAccountActionViewModel;

        KeeperDataModel = keeperDataModel;
    }

    public async Task AddFolder()
    {
        var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
            "", ShellPartsBinder.SelectedAccountItemModel);
        accountItemModel.IsFolder = true;
        var oneFolderVm = new OneFolderViewModel();
        oneFolderVm.Initialize(accountItemModel, true);
        await WindowManager.ShowDialogAsync(oneFolderVm);
        if (!oneFolderVm.IsSavePressed) return;

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
            _oneAccountViewModel.Initialize(accountItemModel, true);
            await WindowManager.ShowDialogAsync(_oneAccountViewModel);
            if (!_oneAccountViewModel.IsSavePressed) return;
        }
        else // 2 - счет в банке; 3 - депозит; 4 - карточка 
        {
            accountItemModel.BankAccount = new BankAccountModel { Id = accountItemModel.Id, };
            if (param == 3)
                accountItemModel.BankAccount.Deposit = new Deposit() { Id = accountItemModel.Id };
            if (param == 4)
                accountItemModel.BankAccount.PayCard = new PayCard() { Id = accountItemModel.Id };

            _oneBankAccountViewModel.Initialize(accountItemModel, true, param == 4);
            await WindowManager.ShowDialogAsync(_oneBankAccountViewModel);
            if (!_oneBankAccountViewModel.IsSavePressed) return;
        }

        ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
        KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
    }

    public async Task ChangeAccount()
    {
        var accountItemModel = ShellPartsBinder.SelectedAccountItemModel;
        if (accountItemModel.IsFolder)
        {
            var vm = new OneFolderViewModel();
            vm.Initialize(accountItemModel, false);
            await WindowManager.ShowDialogAsync(vm);
        }
        else if (accountItemModel.IsCard)
        {
            _oneBankAccountViewModel.Initialize(accountItemModel, false, true);
            await WindowManager.ShowDialogAsync(_oneBankAccountViewModel);
        }
        else if (accountItemModel.IsDeposit)
        {
            _oneBankAccountViewModel.Initialize(accountItemModel, false, false);
            await WindowManager.ShowDialogAsync(_oneBankAccountViewModel);
        }
        else if (accountItemModel.IsBankAccount)
        {
            _oneBankAccountViewModel.Initialize(accountItemModel, false, false);
            await WindowManager.ShowDialogAsync(_oneBankAccountViewModel);
        }
        else
        {
            _oneAccountViewModel.Initialize(accountItemModel, false);
            await WindowManager.ShowDialogAsync(_oneAccountViewModel);
        }
    }
    public async void RemoveSelectedAccount()
    {
        await KeeperDataModel.RemoveSelectedAccount();
    }


    public async Task EnrollDepositInterest()
    {
        _depositInterestViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(_depositInterestViewModel);
    }

    public async Task TakeCardFee()
    {
        _cardFeeViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(_cardFeeViewModel);
    }

    public async Task ShowDepositReport()
    {
        _depositReportViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(_depositReportViewModel);
    }

    public async Task ShowVerificationForm()
    {
        _balanceVerificationViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowDialogAsync(_balanceVerificationViewModel);
    }

    public async Task ShowFolderSummaryForm()
    {
        var folderSummaryViewModel = new FolderSummaryViewModel(KeeperDataModel);
        folderSummaryViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowWindowAsync(folderSummaryViewModel);
    }

    public async Task ShowPaymentWaysForm()
    {
        _paymentWaysViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
        await WindowManager.ShowWindowAsync(_paymentWaysViewModel);
    }

    public async Task ShowExpensesOnAccount()
    {
        _expensesOnAccountViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod);
        await WindowManager.ShowDialogAsync(_expensesOnAccountViewModel);
    }
}
