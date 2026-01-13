using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperModels;

namespace KeeperWpf;

public enum AccountCantBeDeletedReasons
{
    CanDelete, IsRoot, HasChildren, HasRelatedTransactions
}

public static class AccountTreeExt
{
    public static AccountItemModel? GetSelectedAccountModel(this KeeperDataModel dataModel)
    {
        foreach (var root in dataModel.AccountsTree)
        {
            var selected = GetSelectedAccountModelInBranch(root);
            if (selected != null) return selected;
        }
        return null;
    }

    private static AccountItemModel? GetSelectedAccountModelInBranch(AccountItemModel branch)
    {
        if (branch.IsSelected) return branch;
        foreach (var child in branch.Children)
        {
            var selected = GetSelectedAccountModelInBranch((AccountItemModel)child);
            if (selected != null) return selected;
        }
        return null;
    }

    public static async Task RemoveSelectedAccount(this KeeperDataModel dataModel)
    {
        var accountModel = dataModel.GetSelectedAccountModel();
        if (accountModel == null) return;
        var windowManager = new WindowManager();
        switch (CheckIfAccountCanBeDeleted(dataModel, accountModel))
        {
            case AccountCantBeDeletedReasons.CanDelete:
                var myMessageBoxViewModel = new MyMessageBoxViewModel(MessageType.Confirmation,
                    new List<string>()
                    {
                        "Проверено, счет не используется в транзакциях.",
                        "Удаление счета", "",
                        $"<<{accountModel.Name}>>", "",
                        "Удалить?"
                    });
                var answer = await windowManager.ShowDialogAsync(myMessageBoxViewModel);
                if (answer.Value)
                    RemoveAccountLowLevel(dataModel, accountModel);
                break;
            case AccountCantBeDeletedReasons.IsRoot:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    "Корневой счет нельзя удалять!"));
                break;
            case AccountCantBeDeletedReasons.HasChildren:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { "Разрешено удалять", "", "только конечные листья дерева счетов!" }, -1));
                break;
            case AccountCantBeDeletedReasons.HasRelatedTransactions:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    "Этот счет используется в проводках!"));
                break;
        }
    }

    private static AccountCantBeDeletedReasons CheckIfAccountCanBeDeleted(this KeeperDataModel dataModel, AccountItemModel account)
    {
        if (account.Parent == null) return AccountCantBeDeletedReasons.IsRoot;
        if (account.Children.Any()) return AccountCantBeDeletedReasons.HasChildren;
        if (dataModel.Transactions.Values.Any(t =>
            t.MyAccount.Id == account.Id ||
            (t.MySecondAccount != null && t.MySecondAccount.Id == account.Id) ||
            t.Tags != null && t.Tags.Select(tag => tag.Id).Contains(account.Id)))
            return AccountCantBeDeletedReasons.HasRelatedTransactions;
        return AccountCantBeDeletedReasons.CanDelete;
    }

    private static void RemoveAccountLowLevel(this KeeperDataModel dataModel, AccountItemModel accountItemModel)
    {
        var owner = accountItemModel.Parent;
        accountItemModel.Parent = null;
        owner.Children.Remove(accountItemModel);
        dataModel.AcMoDict.Remove(accountItemModel.Id);
    }
}