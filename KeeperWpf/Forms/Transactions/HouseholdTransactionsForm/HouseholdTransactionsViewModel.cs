using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class HouseholdTransactionsViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private readonly AccNameSelector _accNameSelectionControlInitializer;
    private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

    public TransactionModel TranInWork { get; set; } = new TransactionModel();

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

    private string _myAccountBalance = null!;
    public string MyAccountBalance
    {
        get => _myAccountBalance;
        set
        {
            if (value == _myAccountBalance) return;
            _myAccountBalance = value;
            NotifyOfPropertyChange();
        }
    }

    public List<PaymentWay> PaymentWays { get; set; }

    private PaymentWay _selectedPaymentWay;
    public PaymentWay SelectedPaymentWay
    {
        get => _selectedPaymentWay;
        set
        {
            _selectedPaymentWay = value;
            TranInWork.PaymentWay = value;
            NotifyOfPropertyChange();
        }
    }

    public List<HouseholdPaymentTemplate> PaymentTemplates { get; set; }

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


    public HouseholdTransactionsViewModel(KeeperDataModel dataModel,
        BalanceDuringTransactionHinter balanceDuringTransactionHinter,
             AccNameSelector accNameSelectionControlInitializer)
    {
        _dataModel = dataModel;
        _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
        _balanceDuringTransactionHinter = balanceDuringTransactionHinter;

        PaymentWays = Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList();
        PaymentTemplates = new HouseholdPaymentsFactory(_dataModel).Create();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Оплата коммунальных";
    }

    public void Initialize(DateTime timestamp)
    {
        TranInWork.Operation = OperationType.Расход;
        TranInWork.MyAccount = _dataModel.AcMoDict[1137]; // БНБ 1-2-3 виртуальная
        TranInWork.Currency = TranInWork.MyAccount.BankAccount!.MainCurrency;
        TranInWork.PaymentWay = PaymentWay.ОплатаПоЕрип;
        TranInWork.Timestamp = timestamp;
        SelectedPaymentWay = TranInWork.PaymentWay;

        MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyAccount(TranInWork);
        MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

        MyAccountBalance = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);

        MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
        MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;
    }

    private void MyAccNameSelectorVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "MyAccName")
        {
            TranInWork.MyAccount = _dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName!.Id];
            MyAccountBalance = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        }
    }

    private void MyDatePickerVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var selectedDate = MyDatePickerVm.SelectedDate;
        var dayTransactions = _dataModel.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

        int minute = 1;
        if (dayTransactions.Any())
            minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;

        TranInWork.Timestamp = selectedDate.Date.AddMinutes(minute);
    }

    public List<TransactionModel> GetTransactions()
    {
        var result = new List<TransactionModel>();
        var timestamp = TranInWork.Timestamp;
        foreach (var template in PaymentTemplates.Where(t => t.IsOn))
        {
            var tran = new TransactionModel(); 
            tran.Operation = OperationType.Расход;
            tran.MyAccount = TranInWork.MyAccount;
            tran.Timestamp = timestamp;
            tran.Counterparty = template.Counterparty;
            tran.Category = template.Category;
            tran.Amount = template.Amount;
            tran.Currency = template.Currency;
            tran.Tags = template.Tags;
            tran.Comment = template.Comment;
            result.Add(tran);

            timestamp = timestamp.AddMinutes(1);
        }
        return result;
    }

    public async Task Save()
    {
        await TryCloseAsync(true);
    }

    public async Task Cancel()
    {
        await TryCloseAsync(false);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return await base.CanCloseAsync(cancellationToken);
    }
}
