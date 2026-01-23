using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;

namespace KeeperWpf;

public class MainMenuViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel, ShellPartsBinder shellPartsBinder,
    TransactionsViewModel transactionsViewModel, RatesViewModel ratesViewModel,
    MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel,
    // charts:
    BalancesAndSaldosViewModel balancesAndSaldosViewModel, ExpenseByCategoriesViewModel expenseByCategoriesViewModel,
    DepoCurrResultViewModel depoCurrResultViewModel,
    //
    SalaryViewModel salaryViewModel, GskViewModel gskViewModel, CarsViewModel carsViewModel, 
    OpenDepositsViewModel openDepositsViewModel, CardsAndAccountsViewModel cardsAndAccountsViewModel,
    // trust:
    InvestmentAssetsViewModel investmentAssetsViewModel, AssetRatesViewModel assetRatesViewModel,
    TrustAccountsViewModel trustAccountsViewModel, InvestmentTransactionsViewModel investmentTransactionsViewModel,
    InvestmentAnalysisViewModel investmentAnalysisViewModel,
    //
    MemosViewModel memosViewModel, SettingsViewModel settingsViewModel, 
    ButtonCollectionBuilderViewModel buttonCollectionBuilderViewModel, ToTxtSaver toTxtSaver)
    : PropertyChangedBase
{

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

    private string _bellPath = "../../Resources/mainmenu/white-bell.png";
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
        var hasAlarm = keeperDataModel.HasLowBalanceAlarm();
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

    // UserControl должен быть Focusable="True" чтобы шорткаты работали
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
            transactionsViewModel.Initialize();

            // после добавления переносов в поле комента стала криво рассчитывать отрисовку
            // поэтому говорим диспетчеру, что когда он будет абсолютно свободен (т.е. отрисует форму)
            // надо изменить ширину формы -  это заставит wpf перерисовать форму, на этот раз правильно
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new System.Action(() =>
            {
                transactionsViewModel.Width -= 1;
            }));
            await windowManager.ShowDialogAsync(transactionsViewModel);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        if (transactionsViewModel.Model.IsCollectionChanged)
        {
            shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
            await keeperDataModel.RefreshCardBalances();
            SetBellPath();
            await SaveInTextFilesForBackup();
        }
    }

    public async Task ShowRatesForm()
    {
        await ratesViewModel.Initialize();
        await windowManager.ShowDialogAsync(ratesViewModel);

        //_shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
        await SaveInTextFilesForBackup();
    }

    public async Task ShowMonthAnalysisForm()
    {
        monthAnalysisViewModel.Initialize();
        await windowManager.ShowDialogAsync(monthAnalysisViewModel);
    }

    public async Task ShowDepositOffersForm()
    {
        bankOffersViewModel.Initialize();
        await windowManager.ShowDialogAsync(bankOffersViewModel);
    }

    public async Task ShowBalancesAndSaldosChart()
    {
        balancesAndSaldosViewModel.Initialize(keeperDataModel);
        await windowManager.ShowWindowAsync(balancesAndSaldosViewModel);
    }

    public async Task ShowExpensesChart()
    {
        expenseByCategoriesViewModel.Initialize();
        await windowManager.ShowDialogAsync(expenseByCategoriesViewModel);
    }

    public async Task ShowDepoPlusCurrenciesChart()
    {
        depoCurrResultViewModel.Initialize();
        await windowManager.ShowDialogAsync(depoCurrResultViewModel);
    }

    public async Task ShowSalaryForm()
    {
        salaryViewModel.Initialize();
        await windowManager.ShowDialogAsync(salaryViewModel);
    }

    public async Task ShowGskForm()
    {
        gskViewModel.Initialize();
        await windowManager.ShowDialogAsync(gskViewModel);
    }

    public async Task ShowCarForm()
    {
        carsViewModel.Initialize();
        await windowManager.ShowDialogAsync(carsViewModel);
    }

    public async Task ShowDepositsForm()
    {
        openDepositsViewModel.Initialize();
        await windowManager.ShowWindowAsync(openDepositsViewModel);
    }

    public async Task ShowPayCardsForm()
    {
        cardsAndAccountsViewModel.Initialize();
        await windowManager.ShowDialogAsync(cardsAndAccountsViewModel);
    }

    #region Investments
    public async Task ShowTrustAccountsForm()
    {
        trustAccountsViewModel.Initialize();
        await windowManager.ShowWindowAsync(trustAccountsViewModel);
    }

    public async Task ShowInvestmentAssetsForm()
    {
        investmentAssetsViewModel.Initialize();
        await windowManager.ShowWindowAsync(investmentAssetsViewModel);
    }

    public async Task ShowAssetRatesForm()
    {
        assetRatesViewModel.Initialize();
        await windowManager.ShowWindowAsync(assetRatesViewModel);
    }

    public async Task ShowInvestmentTransactionsForm()
    {
        investmentTransactionsViewModel.Initialize();
        await windowManager.ShowWindowAsync(investmentTransactionsViewModel);
        shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
        await SaveInTextFilesForBackup();
    }

    public async Task ShowInvestmentAnalysisForm()
    {
        investmentAnalysisViewModel.Initialize();
        await windowManager.ShowWindowAsync(investmentAnalysisViewModel);
    }
    #endregion

    public async Task ShowRemindersForm()
    {
        await memosViewModel.Initialize();
        await windowManager.ShowDialogAsync(memosViewModel);
    }

    public async Task ShowSettingsForm()
    {
        await windowManager.ShowDialogAsync(settingsViewModel);
    }

    public async Task ShowButtonCollectionBuilder()
    {
        buttonCollectionBuilderViewModel.Initialize();
        await windowManager.ShowDialogAsync(buttonCollectionBuilderViewModel);
    }

    public async Task SaveInTextFilesForBackup()
    {
        shellPartsBinder.FooterVisibility = Visibility.Visible;
        try
        {
            var exception = await toTxtSaver.Save();
            if (exception != null)
            {
                MessageBox.Show("Ошибка при сохранении резервной копии в текстовые файлы: " + exception.Message);
                return;
            }

            var zipException = await toTxtSaver.ZipTxtFiles();
            if (zipException != null)
            {
                MessageBox.Show("Ошибка при архивировании текстовых файлов резервной копии: " + zipException.Message);
                return;
            }

            var deleteException = await toTxtSaver.DeleteTxtFiles();
            if (deleteException != null)
            {
                MessageBox.Show("Ошибка при удалении текстовых файлов резервной копии: " + deleteException.Message);
                return;
            }
        }
        finally
        {
            shellPartsBinder.FooterVisibility = Visibility.Hidden;
        }
    }

    public void Calculator()
    {
        System.Diagnostics.Process.Start("calc");
    }
}
