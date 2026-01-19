using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;

namespace KeeperWpf;

public class MainMenuViewModel : PropertyChangedBase
{
    private readonly IWindowManager _windowManager;
    private readonly KeeperDataModel _keeperDataModel;
    private readonly ShellPartsBinder _shellPartsBinder;
    private readonly RatesViewModel _ratesViewModel;
    private readonly MonthAnalysisViewModel _monthAnalysisViewModel;
    private readonly TransactionsViewModel _transactionsViewModel;
    private readonly BankOffersViewModel _bankOffersViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly MemosViewModel _memosViewModel;
    private readonly CarsViewModel _carsViewModel;
    private readonly ExpenseByCategoriesViewModel _expenseByCategoriesViewModel;
    private readonly DepoCurrResultViewModel _depoCurrResultViewModel;
    private readonly GskViewModel _gskViewModel;
    private readonly OpenDepositsViewModel _openDepositsViewModel;
    private readonly CardsAndAccountsViewModel _cardsAndAccountsViewModel;
    private readonly SalaryViewModel _salaryViewModel;
    private readonly InvestmentAssetsViewModel _investmentAssetsViewModel;
    private readonly AssetRatesViewModel _assetRatesViewModel;
    private readonly TrustAccountsViewModel _trustAccountsViewModel;
    private readonly InvestmentTransactionsViewModel _investmentTransactionsViewModel;
    private readonly InvestmentAnalysisViewModel _investmentAnalysisViewModel;
    private readonly ButtonCollectionBuilderViewModel _buttonCollectionBuilderViewModel;
    private readonly ToTxtSaver _toTxtSaver;

    #region Reminder icon
    private Visibility _reminderIconVisibility = Visibility.Collapsed;
    public Visibility ReminderIconVisibility
    {
        get => _reminderIconVisibility;
        set
        {
            if (value == _reminderIconVisibility) return;
            _reminderIconVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    private Visibility _reminderWaitIconVisibility = Visibility.Visible;
    public Visibility ReminderWaitIconVisibility
    {
        get => _reminderWaitIconVisibility;
        set
        {
            if (value == _reminderWaitIconVisibility) return;
            _reminderWaitIconVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    private string _bellPath;
    public string BellPath
    {
        get => _bellPath;
        set
        {
            if (value == _bellPath) return;
            _bellPath = value;
            NotifyOfPropertyChange();
        }
    }

    public void SetBellPath()
    {
        var hasAlarm = _keeperDataModel.HasLowBalanceAlarm();
        BellPath = hasAlarm ? "../../Resources/mainmenu/yellow-bell.png" : "../../Resources/mainmenu/white-bell.png";
        ReminderWaitIconVisibility = Visibility.Collapsed;
        ReminderIconVisibility = Visibility.Visible;
    }
    #endregion

    #region Exchange icon
    private Visibility _exchangeWaitIconVisibility = Visibility.Collapsed;
    public Visibility ExchangeWaitIconVisibility
    {
        get => _exchangeWaitIconVisibility;
        set
        {
            if (value == _exchangeWaitIconVisibility) return;
            _exchangeWaitIconVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    private Visibility _exchangeIconVisibility = Visibility.Visible;

    public Visibility ExchangeIconVisibility
    {
        get => _exchangeIconVisibility;
        set
        {
            if (value == _exchangeIconVisibility) return;
            _exchangeIconVisibility = value;
            NotifyOfPropertyChange();
        }
    }

    public void SetExchangeIcon()
    {
        ExchangeWaitIconVisibility = Visibility.Collapsed;
        ExchangeIconVisibility = Visibility.Visible;
    }
    #endregion

    public MainMenuViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel,
        ShellPartsBinder shellPartsBinder,
        TransactionsViewModel transactionsViewModel,
        RatesViewModel ratesViewModel,
        MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel,
         SettingsViewModel settingsViewModel, MemosViewModel memosViewModel,
        CarsViewModel carsViewModel, ExpenseByCategoriesViewModel expenseByCategoriesViewModel,
        DepoCurrResultViewModel depoCurrResultViewModel, GskViewModel gskViewModel,
        OpenDepositsViewModel openDepositsViewModel,
        CardsAndAccountsViewModel cardsAndAccountsViewModel, SalaryViewModel salaryViewModel,
        InvestmentAssetsViewModel investmentAssetsViewModel, AssetRatesViewModel assetRatesViewModel,
        TrustAccountsViewModel trustAccountsViewModel, InvestmentTransactionsViewModel investmentTransactionsViewModel,
        InvestmentAnalysisViewModel investmentAnalysisViewModel, ButtonCollectionBuilderViewModel buttonCollectionBuilderViewModel,
        ToTxtSaver toTxtSaver)
    {
        _windowManager = windowManager;
        _keeperDataModel = keeperDataModel;
        _shellPartsBinder = shellPartsBinder;
        _ratesViewModel = ratesViewModel;
        _monthAnalysisViewModel = monthAnalysisViewModel;
        _transactionsViewModel = transactionsViewModel;
        _bankOffersViewModel = bankOffersViewModel;
        _settingsViewModel = settingsViewModel;
        _memosViewModel = memosViewModel;
        _carsViewModel = carsViewModel;
        _expenseByCategoriesViewModel = expenseByCategoriesViewModel;
        _depoCurrResultViewModel = depoCurrResultViewModel;
        _gskViewModel = gskViewModel;
        _openDepositsViewModel = openDepositsViewModel;
        _cardsAndAccountsViewModel = cardsAndAccountsViewModel;
        _salaryViewModel = salaryViewModel;
        _investmentAssetsViewModel = investmentAssetsViewModel;
        _assetRatesViewModel = assetRatesViewModel;
        _trustAccountsViewModel = trustAccountsViewModel;
        _investmentTransactionsViewModel = investmentTransactionsViewModel;
        _investmentAnalysisViewModel = investmentAnalysisViewModel;
        _buttonCollectionBuilderViewModel = buttonCollectionBuilderViewModel;
        _toTxtSaver = toTxtSaver;
    }

    // for short-cuts
    public async Task OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.O:
                await ShowTransactionsForm();
                break;
            case Key.C:
                await ShowRatesForm();
                break;
            case Key.A:
                await ShowMonthAnalysisForm();
                break;
            case Key.B:
                await ShowDepositOffersForm();
                break;
            case Key.D:
                await ShowDepositsForm();
                break;
            case Key.P:
                await ShowPayCardsForm();
                break;
            case Key.F10:
                await ShowSettingsForm();
                break;
            case Key.S:
                await SaveInTextFilesForBackup();
                break;
            case Key.E:
                break;
        }
        // to prevent beeping on Enter key
        if (e.Key == Key.Enter)
            e.Handled = true;
    }

    public async Task ShowTransactionsForm()
    {
        try
        {
            _transactionsViewModel.Initialize();

            // после добавления переносов в поле комента стала криво рассчитывать отрисовку
            // поэтому говорим диспетчеру, что когда он будет абсолютно свободен (т.е. отрисует форму)
            // надо изменить ширину формы -  это заставит wpf перерисовать форму, на этот раз правильно
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new System.Action(() =>
            {
                _transactionsViewModel.Width -= 1;
            }));
            await _windowManager.ShowDialogAsync(_transactionsViewModel);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        if (_transactionsViewModel.Model.IsCollectionChanged)
        {
            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
            await _keeperDataModel.RefreshCardBalances();
            SetBellPath();
            await SaveInTextFilesForBackup();
        }
    }

    public async Task ShowRatesForm()
    {
        await _ratesViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_ratesViewModel);
        
        //_shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
        await SaveInTextFilesForBackup();
    }

    public async Task ShowMonthAnalysisForm()
    {
        _monthAnalysisViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_monthAnalysisViewModel);
    }

    public async Task ShowDepositOffersForm()
    {
        _bankOffersViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_bankOffersViewModel);
    }

    public async Task ShowBalancesAndSaldosChart()
    {
        var vm = new BalancesAndSaldosViewModel();
        vm.Initialize(_keeperDataModel);
        await _windowManager.ShowWindowAsync(vm);
    }

    public async Task ShowExpensesChart()
    {
        _expenseByCategoriesViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_expenseByCategoriesViewModel);
    }

    public async Task ShowDepoPlusCurreniesChart()
    {
        _depoCurrResultViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_depoCurrResultViewModel);
    }

    public async Task ShowSalaryForm()
    {
        _salaryViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_salaryViewModel);
    }

    public async Task ShowGskForm()
    {
        _gskViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_gskViewModel);
    }

    public async Task ShowCarForm()
    {
        _carsViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_carsViewModel);
    }

    public async Task ShowDepositsForm()
    {
        _openDepositsViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_openDepositsViewModel);
    }

    public async Task ShowPayCardsForm()
    {
        _cardsAndAccountsViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_cardsAndAccountsViewModel);
    }

    #region Investments
    public async Task ShowTrustAccountsForm()
    {
        _trustAccountsViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_trustAccountsViewModel);
    }

    public async Task ShowInvestmentAssetsForm()
    {
        _investmentAssetsViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_investmentAssetsViewModel);
    }

    public async Task ShowAssetRatesForm()
    {
        _assetRatesViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_assetRatesViewModel);
    }

    public async Task ShowInvestmentTransactionsForm()
    {
        _investmentTransactionsViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_investmentTransactionsViewModel);
        _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
        await SaveInTextFilesForBackup();
    }

    public async Task ShowInvestmentAnalysisForm()
    {
        _investmentAnalysisViewModel.Initialize();
        await _windowManager.ShowWindowAsync(_investmentAnalysisViewModel);
    }
    #endregion


    public async Task SaveInTextFilesForBackup()
    {
        var exception = await _toTxtSaver.Save();
        if (exception != null)
        {
            MessageBox.Show("Ошибка при сохранении резервной копии в текстовые файлы: " + exception.Message);
            return;
        }

        var zipException = await _toTxtSaver.ZipTxtFiles();
        if (zipException != null)
        {
            MessageBox.Show("Ошибка при архивировании текстовых файлов резервной копии: " + zipException.Message);
            return;
        }

        var deleteException = await _toTxtSaver.DeleteTxtFiles();
        if (deleteException != null)
        {
            MessageBox.Show("Ошибка при удалении текстовых файлов резервной копии: " + deleteException.Message);
            return;
        }
    }

    public void Calculator()
    {
        System.Diagnostics.Process.Start("calc");
    }

    public async Task ShowRemindersForm()
    {
        await _memosViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_memosViewModel);
    }

    public async Task ShowSettingsForm()
    {
        await _windowManager.ShowDialogAsync(_settingsViewModel);
    }

    public async Task ShowButtonCollectionBuilder()
    {
        _buttonCollectionBuilderViewModel.Initialize();
        await _windowManager.ShowDialogAsync(_buttonCollectionBuilderViewModel);
    }
}
