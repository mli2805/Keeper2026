using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class BankAccountMemoViewModel(KeeperDataModel keeperDataModel, 
    BankAccountMemosRepository bankAccountMemosRepository) : Screen
{
    public ObservableCollection<BankAccountMemoModel> Rows { get; set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Остатки и платежи по картам";
    }

    public async Task Initialize()
    {
        await RecollectCardsListWithMemos();
        EvaluateBalanceAndMonthPayments();

        Rows = new ObservableCollection<BankAccountMemoModel>(
            keeperDataModel.BankAccountMemoModels.OrderByDescending(b => b.CurrentBalance));
    }

    // добавить только что заведенные новые карточки,
    // удалить закрытые карточки (перенесенные в закрытые)
    private async Task RecollectCardsListWithMemos()
    {
        // 161 - папка Счета и карты - берем только карты в byn
        var all = keeperDataModel.AcMoDict.Values
             .Where(a => a.Is(161) && a.IsCard && a.BankAccount!.MainCurrency == CurrencyCode.BYN).ToList();

        // существующие Memo
        var exist = keeperDataModel.BankAccountMemoModels.Select(b => b.Account.Id).ToHashSet();
        // новые карточки
        var newAccs = all.Where(a => !exist.Contains(a.Id)).ToList();

        foreach (var a in newAccs)
        {
            var depositOffer = keeperDataModel.DepositOffers.First(o => o.Id == a.BankAccount!.DepositOfferId);
            var model = new BankAccountMemoModel()
            {
                Account = a,
                CurrentBalance = keeperDataModel.GetCurrentBalance(a),
                CurrentMonthPayments = keeperDataModel.GetExpenseForCurrentMonth(a),

                Comment = depositOffer.Comment
            };
            keeperDataModel.BankAccountMemoModels.Add(model);
        }

        // Memo от закрытых карточек
        var closed = keeperDataModel.BankAccountMemoModels.Where(m => !all.Any(a => a.Id == m.Account.Id)).ToList();
        foreach (var c in closed)
        {
            keeperDataModel.BankAccountMemoModels.Remove(c);
        }

        await bankAccountMemosRepository.SaveAll(keeperDataModel.BankAccountMemoModels);
    }

    private void EvaluateBalanceAndMonthPayments()
    {
        foreach (var bankAccountMemoModel in keeperDataModel.BankAccountMemoModels)
        {
            bankAccountMemoModel.CurrentBalance = keeperDataModel.GetCurrentBalance(bankAccountMemoModel.Account);
            bankAccountMemoModel.CurrentMonthPayments = keeperDataModel.GetExpenseForCurrentMonth(bankAccountMemoModel.Account);
        }
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await bankAccountMemosRepository.SaveAll(Rows.ToList());
        return await base.CanCloseAsync(cancellationToken);
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
