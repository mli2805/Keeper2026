using Caliburn.Micro;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class CarLoanPayment
{
    public int N { get; set; }
    public string Date { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string Interest { get; set; } = null!;
    public string Payment { get; set; } = null!;
}


[ExportViewModel]
public class CarLoanViewModel(KeeperDataModel dataModel, LoanPaymentViewModel loanPaymentViewModel,
    IWindowManager windowManager) : Screen
{
    public BindableCollection<CarLoanPayment> Rows { get; } = new BindableCollection<CarLoanPayment>();
    public decimal _totalBody;
    public decimal _totalInterest;
    public decimal _totalPayment;


    public string TotalBody => _totalBody.ToString("N2");
    public string TotalInterest => _totalInterest.ToString("N2");
    public string TotalPayment => _totalPayment.ToString("N2");

    public string BodyRemaining => (61400 - _totalBody).ToString("N2");

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Отчет по кредиту";
    }

    public void Initialize()
    {
        var bodyTrans = dataModel.Transactions.Values
            .Where(x => x.Category != null && x.Category.Id == 1145).ToList();
        var interestTrans = dataModel.Transactions.Values
            .Where(x => x.Category != null && x.Category.Id == 1159).ToList();

        Rows.Clear();
        for (int i = 0; i < bodyTrans.Count; i++)
        {
            var bodyTran = bodyTrans[i];
            _totalBody += bodyTran.Amount;
            var interestTran = interestTrans[i];
            _totalInterest += interestTran.Amount;
            _totalPayment += bodyTran.Amount + interestTran.Amount;
            Rows.Add(new CarLoanPayment
            {
                N = i + 1,
                Date = bodyTran.Timestamp.ToString("dd.MM.yyyy"),
                Body = bodyTran.Amount.ToString("N2"),
                Interest = interestTran.Amount.ToString("N2"),
                Payment = (bodyTran.Amount + interestTran.Amount).ToString("N2")
            });
        }
    }

    public async Task InputPayment()
    {
        loanPaymentViewModel.Initialize();
        var result = await windowManager.ShowDialogAsync(loanPaymentViewModel);
        if (result == true)
        {
            Initialize();
            NotifyOfPropertyChange(nameof(TotalBody));
            NotifyOfPropertyChange(nameof(TotalInterest));
            NotifyOfPropertyChange(nameof(TotalPayment));
            NotifyOfPropertyChange(nameof(BodyRemaining));
        }
    }

    public async Task Close()
    {
      await TryCloseAsync();
    }
}
