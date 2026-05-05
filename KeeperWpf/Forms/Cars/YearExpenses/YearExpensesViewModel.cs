using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class YearExpensesViewModel(KeeperDataModel dataModel) : Screen
{
    public BindableCollection<YearExpenseRow> Expenses { get; set; } = [];
    public BindableCollection<CategorySummaryRow> CategorySummaries { get; set; } = [];
    public decimal CategoryTotalUsd { get; set; }
    public string Title { get; set; } = string.Empty;

    public void Initialize(CarModel car, YearMileageModel yearMileage)
    {
        Title = $"{car.Title} — год {yearMileage.YearNumber} ({yearMileage.FromTo})";

        var expenses = dataModel.Transactions.Values
            .Where(t => yearMileage.Period.Includes(t.Timestamp) &&
                        t.Operation == OperationType.Расход &&
                        t.Category!.Parent!.Is(car.CarAccountId) &&
                        t.Tags.All(tag => tag.Id != 1064))
            .OrderByDescending(t => t.GetAmountInUsd(dataModel))
            .Select(t => new YearExpenseRow
            {
                Timestamp = t.Timestamp,
                Category = t.Category,
                Amount = t.Amount,
                Currency = t.Currency,
                AmountInUsd = t.GetAmountInUsd(dataModel),
                Comment = t.Comment
            })
            .ToList();

        Expenses = [.. expenses];

        CategoryTotalUsd = expenses.Sum(e => e.AmountInUsd);

        CategorySummaries = [.. expenses
            .GroupBy(e => e.Category?.Name ?? "—")
            .Select(g => new CategorySummaryRow
            {
                CategoryName = g.Key,
                TotalUsd = g.Sum(e => e.AmountInUsd),
                Percent = CategoryTotalUsd != 0 ? g.Sum(e => e.AmountInUsd) / CategoryTotalUsd * 100 : 0
            })
            .OrderByDescending(r => r.TotalUsd)];
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Расходы за год";
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
