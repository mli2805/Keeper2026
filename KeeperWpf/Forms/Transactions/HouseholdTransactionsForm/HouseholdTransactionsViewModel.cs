using Caliburn.Micro;
using KeeperInfrastructure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

[ExportViewModel]
public class HouseholdTransactionsViewModel(KeeperDataModel dataModel, PathFinder pathFinder,
    HouseholdPaymentsFactory householdPaymentsFactory, TransactionsRepository transactionsRepository,
        ComboTreesProvider comboTreesProvider, AccNameSelector accNameSelectionControlInitializer,
        OneMonthReadingsViewModel oneMonthReadingsViewModel, IWindowManager windowManager) : Screen
{
    public UtilityMetersModel UtilityMetersModel1 { get; set; } = null!;
    public List<TransactionModel> PreviousPayments { get; set; } = null!;
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

    private decimal _myAccountBalance;
    public string MyAccountBalanceStr => $"{_myAccountBalance:#,0.00} -> {_myAccountBalance - TranInWork.Amount:#,0.00} ";


    public List<HouseholdPaymentTemplate> PaymentTemplates { get; set; } = null!;

    public string JsonFolder => Path.Combine(pathFinder.GetDataFolder(), "householdbills");
    public string JsonFilePath => Path
        .Combine(pathFinder.GetDataFolder(), "householdbills", DateTime.Today.AddMonths(-1).ToString("yyyyMM") + ".json");
    public BillMonthDocument? Bill { get; set; }
    public string BillTotal { get; set; } = "";
    public string ForecastTotal { get; set; } = "";

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
        DisplayName = "Оплата коммунальных";
    }

    public void Initialize()
    {
        var timestamp = dataModel.Transactions.Values
            .Where(t => t.Timestamp.Date == DateTime.Today.Date)
            .OrderBy(l => l.Timestamp)
            .LastOrDefault()?.Timestamp ?? DateTime.Today.Date;
        PreviousPayments = dataModel.GetPreviousPayments();

        UtilityMetersModel1 = dataModel.GetLastOrNew();
        Bill = GetHouseholdBill();
        BillTotal = $"К оплате: {(Bill?.Footer.TotalAccrued ?? 0):#,0.00} руб.";
        PaymentTemplates = householdPaymentsFactory
            .Create(UtilityMetersModel1, PreviousPayments, Bill?.Footer.TotalAccrued ?? 0);
        ForecastTotal = $"Итого: {PaymentTemplates.Sum(t => t.AmountForecast):#,0.00} руб.";

        comboTreesProvider.Initialize();
        TranInWork.Operation = OperationType.Расход;
        TranInWork.MyAccount = dataModel.AcMoDict[1121]; //Кашалот;   1137 - БНБ 1-2-3 виртуальная
        TranInWork.Currency = TranInWork.MyAccount.BankAccount!.MainCurrency;
        TranInWork.PaymentWay = PaymentWay.ОплатаПоЕрип;
        TranInWork.Timestamp = timestamp;

        MyAccNameSelectorVm = accNameSelectionControlInitializer.ForMyAccount(TranInWork);
        MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

        _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
            TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));

        MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
        MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;
    }

    private BillMonthDocument? GetHouseholdBill()
    {
        if (!File.Exists(JsonFilePath)) return null;
        var json = File.ReadAllText(JsonFilePath);
        return System.Text.Json.JsonSerializer.Deserialize<BillMonthDocument>(json);
    }

    private void MyAccNameSelectorVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "MyAccName")
        {
            TranInWork.MyAccount = dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName!.Id];
            _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));
            NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
        }
    }

    private void MyDatePickerVm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var selectedDate = MyDatePickerVm.SelectedDate;
        var dayTransactions = dataModel.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

        int minute = 1;
        if (dayTransactions.Any())
            minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;

        TranInWork.Timestamp = selectedDate.Date.AddMinutes(minute);
        _myAccountBalance = dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
           TranInWork.MyAccount, TranInWork.MyAccount.BankAccount!.MainCurrency, TranInWork.Timestamp));
        NotifyOfPropertyChange(nameof(MyAccountBalanceStr));
    }

    public async Task InputUtilityMeters()
    {
        oneMonthReadingsViewModel.Initialize();
        var result = await windowManager.ShowDialogAsync(oneMonthReadingsViewModel);
        if (result == true)
        {
            UtilityMetersModel1 = oneMonthReadingsViewModel.UtilityMetersInWork;

            PaymentTemplates = householdPaymentsFactory
                .Create(UtilityMetersModel1, PreviousPayments, Bill?.Footer.TotalAccrued ?? 0);
            ForecastTotal = $"Итого: {PaymentTemplates.Sum(t => t.AmountForecast):#,0.00} руб.";

            NotifyOfPropertyChange(nameof(PaymentTemplates));
            NotifyOfPropertyChange(nameof(ForecastTotal));
            NotifyOfPropertyChange(nameof(UtilityMetersModel1));
        }
    }

    public async Task LoadHouseholdBillPdf()
    {
        var initialDirectory = @"c:\Google Drive\жировки";
        var dialog = new OpenFileDialog
        {
            InitialDirectory = Directory.Exists(initialDirectory) ? initialDirectory : null,
            DefaultExt = ".pdf",
            Filter = "PDF (*.pdf)|*.pdf",
            Title = "Выбрать PDF жировки"
        };
        dialog.ShowDialog();
        var pdfFilePath = dialog.FileName;
        var bill = BillPdfParser.ParseOneFile(pdfFilePath);
        if (bill == null)
        {
            MessageBox.Show("Не удалось распарсить PDF жировки");
            return;
        }
        var billMonthDocument = new BillMonthDocument(
            1, bill.Value.Footer.Period, bill.Value.Rows, bill.Value.Footer,
            pdfFilePath, null, "г. Минск, ул. Яна Чечота, д.21, кв.108", "7075851081");

        var billJsonStore = new BillMonthJsonStore(JsonFolder);
        await billJsonStore.SaveAsync(billMonthDocument);

        if (billMonthDocument != null &&
            !billMonthDocument.Footer.Period.IsMonthTheSame(DateTime.Now.AddMonths(-1)))
        {
            var strs = new List<string> {
                $"Внимание! Выбeрите жировку за {DateTime.Now.AddMonths(-1):MMMM yyyy}",
                "",
                $"Выбранная жировка за {billMonthDocument.Footer.Period:MMMM yyyy}",
                "распаршена и сохранена в json успешно",
                "Выбранная жировка не за текущий месяц и использована не будет.",
                };
            var vm = new MyMessageBoxViewModel(MessageType.Error, strs);
            await windowManager.ShowDialogAsync(vm);
            return;
        }

        Bill = billMonthDocument;
        BillTotal = $"К оплате: {Bill?.Footer.TotalAccrued ?? 0:#,0.00} руб.";
        PaymentTemplates = householdPaymentsFactory
                .Create(UtilityMetersModel1, PreviousPayments, Bill?.Footer.TotalAccrued ?? 0);
        ForecastTotal = $"Итого: {PaymentTemplates.Sum(t => t.AmountForecast):#,0.00} руб.";

        NotifyOfPropertyChange(nameof(Bill));
        NotifyOfPropertyChange(nameof(BillTotal));
        NotifyOfPropertyChange(nameof(PaymentTemplates));
        NotifyOfPropertyChange(nameof(ForecastTotal));
    }

    public List<TransactionModel> ToTransactionModels()
    {
        var result = new List<TransactionModel>();
        var timestamp = TranInWork.Timestamp;
        foreach (var template in PaymentTemplates.Where(t => t.IsOn))
        {
            var tran = new TransactionModel();
            tran.Operation = OperationType.Расход;
            tran.PaymentWay = PaymentWay.ОплатаПоЕрип;
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

    public async Task SaveTransactions()
    {
        var vm = new MyMessageBoxViewModel(MessageType.Confirmation, new List<string> { "Сохранить транзакции?", "" });
        var result = await windowManager.ShowDialogAsync(vm);
        if (result != true) return;

        var addedTransactions = await transactionsRepository
            .AddTransactions(ToTransactionModels(), dataModel.AcMoDict);
        addedTransactions.ForEach(t => dataModel.Transactions.Add(t.Id, t));
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
