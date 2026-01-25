using System.Collections.Generic;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public static class AccountTreeToDomain
{
    public static List<Account> FlattenAccountTree(this KeeperDataModel dataModel)
    {
        var cN = 0;
        return dataModel.AccountsTree.SelectMany(a => FlattenBranch(a, ++cN)).ToList();
    }

    private static List<Account> FlattenBranch(AccountItemModel accountItemModel, int childNumber)
    {
        var account = accountItemModel.FromModel();
        account.ChildNumber = childNumber;
        var result = new List<Account> { account };

        var cN = 0;
        foreach (var child in accountItemModel.Children)
        {
            result.AddRange(FlattenBranch((AccountItemModel)child, ++cN));
        }
        return result;
    }
}