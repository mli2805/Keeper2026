using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public static class AccountTreeMapper
{
   
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