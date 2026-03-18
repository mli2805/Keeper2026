using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class DepositInterestViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
    ComboTreesProvider comboTreesProvider, ShellPartsBinder shellPartsBinder,
    AccNameSelector accNameSelectionControlInitializer, TransactionsRepository transactionsRepository) : Screen
{
    private AccountItemModel _accountItemModel = null!;

    private AccountItemModel _bank = null!;
    public string BankTitle { get; set; } = null!;

    public bool IsPercent { get; set; }
    public bool IsMoneyBack { get; set; }

    public string DepositTitle { get; set; } = null!;

    private decimal _depositBalance;
    private decimal _myNextAccountBalance;
    public string MyNextAccountBalanceStr =>
       $"{_myNextAccountBalance:#,0.00} {DepositCurrency} -> {_myNextAccountBalance + Amount:#,0.00} {DepositCurrency}";
    public string DepositBalanceStr => $"{_depositBalance:#,0.00} {DepositCurrency} -> {_depositBalance + Amount:#,0.00} {DepositCurrency}";

    public string DepositCurrency { get; set; } = null!;

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value == _amount) return;
            _amount = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(DepositBalanceStr));
            NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
        }
    }

    public AccNameSelectorVm MyNextAccNameSelectorVm { get; set; } = null!;

    private bool _isTransferred;

    public bool IsTransferred
    {
        get => _isTransferred;
        set
        {
            if (value == _isTransferred) return;
            _isTransferred = value;
            NotifyOfPropertyChange();
        }
    }

    private DateTime _transactionTimestamp;
    public DatePickerWithTrianglesVm MyDatePickerVm { get; set; } = null!;
    public string Comment { get; set; } = "";

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Начислены проценты/кэшбек";
    }

    public void Initialize(AccountItemModel accountItemModel)
    {
        _accountItemModel = accountItemModel;
        _bank = keeperDataModel.AcMoDict[accountItemModel.BankAccount!.BankId];
        BankTitle = _bank.Name;
        IsMoneyBack = _accountItemModel.IsCard;
        IsPercent = !IsMoneyBack;
        DepositTitle = accountItemModel.Name;
        DepositCurrency = accountItemModel.BankAccount.MainCurrency.ToString().ToUpper();
        comboTreesProvider.Initialize();
        Amount = 0;
        MyNextAccNameSelectorVm = accNameSelectionControlInitializer.ForMyNextAccount();
        MyNextAccNameSelectorVm.PropertyChanged += MyNextAccNameSelectorVm_PropertyChanged;

        MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = DateTime.Today };
        _transactionTimestamp = DateTime.Today.AddDays(1).AddMilliseconds(-1);
        MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;

        _depositBalance = keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
            _accountItemModel, accountItemModel.BankAccount.MainCurrency, _transactionTimestamp));
    }

    private void MyNextAccNameSelectorVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "MyAccName")
        {
            var nextAccountModel = keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
            _myNextAccountBalance = keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                nextAccountModel, _accountItemModel.BankAccount!.MainCurrency, _transactionTimestamp));
            NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
        }
    }

    private void MyDatePickerVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var selectedDate = MyDatePickerVm.SelectedDate;
        var dayTransactions = keeperDataModel.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

        int minute = 1;
        if (dayTransactions.Any())
            minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;

        _transactionTimestamp = selectedDate.Date.AddMinutes(minute);

        _depositBalance = keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
            _accountItemModel, _accountItemModel.BankAccount!.MainCurrency, _transactionTimestamp));
        var nextAccountModel = keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
        _myNextAccountBalance = keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
            nextAccountModel, _accountItemModel.BankAccount!.MainCurrency, _transactionTimestamp));

        NotifyOfPropertyChange(nameof(DepositBalanceStr));
        NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
    }

    public async void Save()
    {
        AccountItemModel? nextAccountItemModel = null;
        if (IsTransferred)
        {
            nextAccountItemModel = keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
            if (_accountItemModel.Id == nextAccountItemModel.Id)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, "Перечисление на самого себя!");
                await windowManager.ShowDialogAsync(vm);
                return;
            }
        }

        var id = keeperDataModel.Transactions.Keys.Max() + 1;
        var thisDateTrans = keeperDataModel.Transactions.Values
            .Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate)
            .OrderBy(l => l.Timestamp)
            .LastOrDefault();
        var timestamp = thisDateTrans?.Timestamp ?? MyDatePickerVm.SelectedDate;
        var tranModel1 = new TransactionModel
        {
            Id = id,
            Timestamp = timestamp.AddMinutes(1),
            Operation = OperationType.Доход,
            MyAccount = _accountItemModel,
            Counterparty = keeperDataModel.AcMoDict[_accountItemModel.BankAccount!.BankId],
            Category = IsPercent ? keeperDataModel.PercentsCategory() : keeperDataModel.MoneyBackCategory(),
            Amount = Amount,
            Currency = _accountItemModel.BankAccount.MainCurrency,
            Tags = new List<AccountItemModel>(),
            Comment = Comment,
        };
        keeperDataModel.Transactions.Add(tranModel1.Id, tranModel1);
        await transactionsRepository.AddTransactions(new List<TransactionModel>() { tranModel1 });

        if (IsTransferred)
        {
            var tranModel2 = new TransactionModel()
            {
                Id = id + 1,
                Timestamp = timestamp.AddMinutes(2),
                Operation = OperationType.Перенос,
                MyAccount = _accountItemModel,
                MySecondAccount = nextAccountItemModel,
                Amount = Amount,
                Currency = _accountItemModel.BankAccount.MainCurrency,
                Tags = new List<AccountItemModel>(),
                Comment = "",
            };
            keeperDataModel.Transactions.Add(tranModel2.Id, tranModel2);
            await transactionsRepository.AddTransactions(new List<TransactionModel>() { tranModel2 });
        }

        shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;

        await TryCloseAsync();
    }

    public async void Cancel()
    {
        await TryCloseAsync();
    }
}
