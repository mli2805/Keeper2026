using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KeeperWpf;

public class BankAccountMemoViewModel(KeeperDataModel keeperDataModel, BankAccountMemosRepository bankAccountMemosRepository) : Screen
{
    public ObservableCollection<BankAccountMemoModel> Rows { get; set; } = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Bank account memo";
    }

    public void Initialize()
    {
        Rows = new ObservableCollection<BankAccountMemoModel>(keeperDataModel.BankAccountMemoModels);
        foreach (var bankAccountMemoModel in Rows)
        {
            bankAccountMemoModel.CurrentBalance = keeperDataModel.GetCurrentBalance(bankAccountMemoModel.Account);
            bankAccountMemoModel.CurrentMonthPayments = keeperDataModel.GetExpenseForCurrentMonth(bankAccountMemoModel.Account);
        }
    }

    public void CardsBalance()
    {
        var sortedList = Rows
            .Where(r=>r.Account.BankAccount!.MainCurrency == CurrencyCode.BYN)
            .OrderByDescending(r => r.CurrentBalance)
            .ToList();
        Rows.Clear();
        foreach (var m in sortedList)
        {
            Rows.Add(m);
        }
    }

    public void AddNew()
    {
        // 161 - папка Счета и карты
        var all = keeperDataModel.AcMoDict.Values
             .Where(a => a.Is(161) && !a.IsFolder);

        var exist = Rows.Select(b => b.Account.Id).ToHashSet();
        var newAccs = all.Where(a => !exist.Contains(a.Id)).ToList();

        foreach (var a in newAccs)
        {
            var model = new BankAccountMemoModel()
            {
                Account = a, 
                CurrentBalance = keeperDataModel.GetCurrentBalance(a), 
                CurrentMonthPayments = keeperDataModel.GetExpenseForCurrentMonth(a),
            };
            Rows.Add(model);
        }
    }

    public async Task Save()
    {
        // если отфильтровано, то удалит отфильтрованное - надо восстанавливать?
        await bankAccountMemosRepository.SaveAll(Rows.ToList());
    }
    
}
