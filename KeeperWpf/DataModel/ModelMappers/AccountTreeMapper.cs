using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public static class AccountTreeMapper
{
    public static void FillInAccountTreeAndDict(this KeeperDataModel dataModel,
        List<Account> accountPlaneList, List<BankAccount> bankAccounts,
        List<Deposit> deposits, List<PayCard> payCards)
    {
        dataModel.AcMoDict = accountPlaneList.Select(a => a.ToModel()).ToDictionary(a => a.Id, a => a);
        dataModel.AccountsTree = new ObservableCollection<AccountItemModel>();

        // идем по списку accounts, но дерево строим из моделей
        foreach (var account in accountPlaneList.OrderBy(a=> a.ParentId).ThenBy(a=>a.ChildNumber))
        {
            AccountItemModel accountItemModel = dataModel.AcMoDict[account.Id];

            var bankAccount = bankAccounts.FirstOrDefault(b => b.Id == accountItemModel.Id);
            if (bankAccount != null)
            {
                accountItemModel.BankAccount = bankAccount.ToModel();
                accountItemModel.BankAccount.Deposit = deposits.FirstOrDefault(d => d.Id == accountItemModel.Id);
                accountItemModel.BankAccount.PayCard = payCards.FirstOrDefault(c => c.Id == accountItemModel.Id);
            }

            if (account.ParentId == 0)
            {
                dataModel.AccountsTree.Add(accountItemModel);
            }
            else
            {
                var parentModel = dataModel.AcMoDict[account.ParentId];
                var childModel = accountItemModel;
                parentModel.Children.Add(childModel);
                childModel.Parent = parentModel;
            }
        }
    }

    public static IEnumerable<Account> FlattenAccountTree(this KeeperDataModel dataModel)
    {
        return dataModel.AccountsTree.SelectMany(FlattenBranch);
    }

    private static IEnumerable<Account> FlattenBranch(AccountItemModel accountItemModel)
    {
        var result = new List<Account> { accountItemModel.FromModel() };
        foreach (var child in accountItemModel.Children)
        {
            result.AddRange(FlattenBranch((AccountItemModel)child));
        }
        return result;
    }
}