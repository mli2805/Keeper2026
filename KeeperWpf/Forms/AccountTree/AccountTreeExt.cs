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
    private static AccountItemModel? GetSelectedAccountModel(this KeeperDataModel dataModel)
    {
        return dataModel.AccountsTree
            .Select(root => GetSelectedAccountModelInBranch(root)).OfType<AccountItemModel>().FirstOrDefault();
    }

    private static AccountItemModel? GetSelectedAccountModelInBranch(AccountItemModel branch)
    {
        if (branch.IsSelected) return branch;
        return branch.Children
            .Select(child => GetSelectedAccountModelInBranch((AccountItemModel)child)).OfType<AccountItemModel>()
            .FirstOrDefault();
    }

    public static async Task<int> RemoveSelectedAccount(this KeeperDataModel dataModel)
    {
        var accountModel = dataModel.GetSelectedAccountModel();
        if (accountModel == null) return -1;
        var windowManager = new WindowManager();
        switch (dataModel.CheckIfAccountCanBeDeleted(accountModel))
        {
            case AccountCantBeDeletedReasons.CanDelete:
                var myMessageBoxViewModel = new MyMessageBoxViewModel(MessageType.Confirmation,
                [
                    "Проверено, счет не используется в транзакциях.",
                    "Удаление счета", "",
                    $"<<{accountModel.Name}>>", "",
                    "Удалить?"
                ]);
                var answer = await windowManager.ShowDialogAsync(myMessageBoxViewModel);
                if (answer != null && answer.Value)
                {
                    dataModel.RemoveAccountLowLevel(accountModel);
                    return accountModel.Id;
                }
                break;
            case AccountCantBeDeletedReasons.IsRoot:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    "Корневой счет нельзя удалять!"));
                break;
            case AccountCantBeDeletedReasons.HasChildren:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    ["Разрешено удалять", "", "только конечные листья дерева счетов!"], -1));
                break;
            case AccountCantBeDeletedReasons.HasRelatedTransactions:
                await windowManager.ShowDialogAsync(new MyMessageBoxViewModel(MessageType.Error,
                    "Этот счет используется в проводках!"));
                break;
        }

        return -1;
    }

    private static AccountCantBeDeletedReasons CheckIfAccountCanBeDeleted(this KeeperDataModel dataModel, AccountItemModel account)
    {
        if (account.Parent == null) return AccountCantBeDeletedReasons.IsRoot;
        if (account.Children.Any()) return AccountCantBeDeletedReasons.HasChildren;
        if (dataModel.Transactions.Values.Any(t =>
            t.MyAccount.Id == account.Id ||
            (t.MySecondAccount != null && t.MySecondAccount.Id == account.Id) ||
            t.Tags.Select(tag => tag.Id).Contains(account.Id)))
            return AccountCantBeDeletedReasons.HasRelatedTransactions;
        return AccountCantBeDeletedReasons.CanDelete;
    }

    private static void RemoveAccountLowLevel(this KeeperDataModel dataModel, AccountItemModel accountItemModel)
    {
        var owner = accountItemModel.Parent;
        accountItemModel.Parent = null;
        owner!.Children.Remove(accountItemModel);
        dataModel.AcMoDict.Remove(accountItemModel.Id);
    }
}