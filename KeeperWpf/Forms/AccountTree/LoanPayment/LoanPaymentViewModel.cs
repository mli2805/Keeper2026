using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class LoanPaymentViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider,
        AccNameSelector accNameSelectionControlInitializer,
        TransactionsRepository transactionsRepository, ShellPartsBinder shellPartsBinder) : Screen
{
    public TransactionModel TranInWork { get; set; } = null!;

    private AccNameSelectorVm _myAccNameSelectorVm = null!;
    public AccNameSelectorVm MyAccNameSelectorVm
    {
        get => _myAccNameSelectorVm;
        set
        {
            if (Equals(value, _myAccNameSelectorVm)) return;
            _myAccNameSelectorVm = value;
            NotifyOfPropertyChange();
        }
    }

    private decimal _myAccountBalance;
    public string MyAccountBalanceStr => $"{_myAccountBalance:#,0.00} -> {_myAccountBalance - TranInWork.Amount:#,0.00} ";
  
    private decimal _amountBody = 511.67m;
    public decimal AmountBody
    {
        get => _amountBody;
        set
        {
            if (value == _amountBody) return;
            _amountBody = value;
            NotifyOfPropertyChange();
        }
    }

    private decimal _amountInterest;
    public decimal AmountInterest
    {
        get => _amountInterest;
        set
        {
            if (value == _amountInterest) return;
            _amountInterest = value;
            NotifyOfPropertyChange();
        }
    }

    private string  _comment1 = string.Empty;
    public string  Comment1
    {   
        get { return _comment1; }
        set { _comment1 = value; NotifyOfPropertyChange(); }
    }

    private string  _comment2 = string.Empty;
    public string  Comment2
    {   
        get { return _comment2; }
        set { _comment2 = value; NotifyOfPropertyChange(); }
    }

    private DatePickerWithTrianglesVm _myDatePickerVm = null!;
    public DatePickerWithTrianglesVm MyDatePickerVm
    {
        get => _myDatePickerVm;
        set
        {
            if (Equals(value, _myDatePickerVm)) return;
            _myDatePickerVm = value;
            NotifyOfPropertyChange();
        }
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Ежемесячный платеж по кредиту";
    }

    public void Initialize()
    {
        comboTreesProvider.Initialize();
        TranInWork = new TransactionModel()
        {
            Operation = OperationType.Расход,
            PaymentWay = PaymentWay.ОплатаПоЕрип,
            MyAccount = dataModel.AcMoDict[1121], // Кашалот
            Timestamp = DateTime.Today.AddMinutes(999),
        };
        TranInWork.PropertyChanged += TranInWork_PropertyChanged;       

        MyAccNameSelectorVm = accNameSelectionControlInitializer.ForMyAccount(TranInWork);
        MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

        _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
            TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));

        MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
        MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;

        this.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(AmountBody) || e.PropertyName == nameof(AmountInterest))
            {
                TranInWork.Amount = AmountBody + AmountInterest;
                NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
            }
        };
    }

    private void TranInWork_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Amount")
        {
            _amountInterest = TranInWork.Amount - AmountBody;
            NotifyOfPropertyChange(nameof(AmountInterest));

            _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));
            NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
        }   
    }

    private void MyAccNameSelectorVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "MyAccName")
        {
            TranInWork.MyAccount = dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName!.Id];
            _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));
            NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
        }
    }

    private void MyDatePickerVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var selectedDate = MyDatePickerVm.SelectedDate;
        TranInWork.Timestamp = selectedDate.Date.AddMinutes(999);
        _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
               TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));
        NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
    }

    public async void Save()
    {
        var thisDateTrans = dataModel.Transactions.Values
            .Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate)
            .OrderBy(l => l.Timestamp)
            .LastOrDefault();
        var timestamp = thisDateTrans?.Timestamp ?? MyDatePickerVm.SelectedDate;

        var categoryBody = 1145; // Погашение тела кредита
        var tranModel1 = CreatePayment(timestamp.AddMinutes(1), _amountBody, categoryBody, Comment1);
        var categoryInterest = 1159; // Погашение процентов по кредиту
        var tranModel2 = CreatePayment(timestamp.AddMinutes(2), _amountInterest, categoryInterest, Comment2); 
        
        var addedTransactions = await transactionsRepository.AddTransactions(new List<TransactionModel>() { tranModel1, tranModel2 }, dataModel.AcMoDict);
        addedTransactions.ForEach(tr => dataModel.Transactions[tr.Id] = tr);

        shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now; // обновление балансов в дереве счетов

        await TryCloseAsync(true);
    }

    private TransactionModel CreatePayment(DateTime timestamp, decimal amount, int categoryId, string comment)
    {
        return new TransactionModel()
        {
            Timestamp = timestamp,
            Operation = OperationType.Расход,
            PaymentWay = PaymentWay.ОплатаПоЕрип,
            MyAccount = TranInWork.MyAccount,
            Counterparty = dataModel.AcMoDict[401], // БГПБ
            Category = dataModel.AcMoDict[categoryId], 
            Amount = amount,
            Currency = CurrencyCode.BYN,
            Tags = new List<AccountItemModel>() { dataModel.AcMoDict[1064] }, // Покупка-продажа авто
            Comment = comment,
        };
    }

    public async Task Cancel()
    {
        await TryCloseAsync(false);
    }

}
