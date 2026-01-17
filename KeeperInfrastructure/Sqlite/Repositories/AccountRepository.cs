using KeeperDomain;
using KeeperModels;
using System.Collections.ObjectModel;

namespace KeeperInfrastructure;

public class AccountRepository(KeeperDbContext keeperDbContext)
{
    public async Task<(ObservableCollection<AccountItemModel>, Dictionary<int, AccountItemModel>)?> GetAccountModelsTreeAndDict()
    {
        var accountPlaneList = await GetAllAccounts();
        if (accountPlaneList.Count == 0)
        {
            return null;
        }
        var bankAccountModels = await GetAllBankAccountsFromDb();
        var deposits = await GetAllDeposits();
        var payCards = await GetAllPayCards();
        var acMoDict = accountPlaneList.Select(ToModel).ToDictionary(a => a.Id, a => a);
        var accountsTree = new ObservableCollection<AccountItemModel>();

        // идем по списку accounts, но дерево строим из моделей
        foreach (var account in accountPlaneList.OrderBy(a => a.ParentId).ThenBy(a => a.ChildNumber))
        {
            AccountItemModel accountItemModel = acMoDict[account.Id];

            var bankAccountModel = bankAccountModels.FirstOrDefault(b => b.Id == accountItemModel.Id);
            if (bankAccountModel != null)
            {
                accountItemModel.BankAccount = bankAccountModel;
                accountItemModel.BankAccount.Deposit = deposits.FirstOrDefault(d => d.Id == accountItemModel.Id);
                accountItemModel.BankAccount.PayCard = payCards.FirstOrDefault(c => c.Id == accountItemModel.Id);
            }

            if (account.ParentId == 0)
            {
                accountsTree.Add(accountItemModel);
            }
            else
            {
                var parentModel = acMoDict[account.ParentId];
                var childModel = accountItemModel;
                parentModel.Children.Add(childModel);
                childModel.Parent = parentModel;
            }
        }

        return (accountsTree, acMoDict);
    }

    private AccountItemModel ToModel(Account account)
    {
        return new AccountItemModel(account.Id, account.Name, null)
        {
            ChildNumber = account.ChildNumber,
            Id = account.Id,
            IsFolder = account.IsFolder,
            IsExpanded = account.IsExpanded,
            AssociatedIncomeId = account.AssociatedIncomeId,
            AssociatedExpenseId = account.AssociatedExpenseId,
            AssociatedExternalId = account.AssociatedExternalId,
            AssociatedTagId = account.AssociatedTagId,
            ShortName = account.ShortName,
            ButtonName = account.ButtonName,
            Comment = account.Comment,
        };
    }

    public async Task<List<Account>> GetAllAccounts()
    {
        return keeperDbContext.Accounts.Select(a => a.FromEf()).ToList();
    }

    public async Task<List<BankAccountModel>> GetAllBankAccountsFromDb()
    {
        return keeperDbContext.BankAccounts.Select(bankAccount => new BankAccountModel
        {
            Id = bankAccount.Id,
            BankId = bankAccount.BankId,
            DepositOfferId = bankAccount.DepositOfferId,
            MainCurrency = bankAccount.MainCurrency,
            AgreementNumber = bankAccount.AgreementNumber,
            ReplenishDetails = bankAccount.ReplenishDetails,
            StartDate = bankAccount.StartDate,
            FinishDate = bankAccount.FinishDate,
            IsMine = bankAccount.IsMine,
        }).ToList();
    }

    public async Task<List<BankAccount>> GetAllBankAccounts()
    {
        return keeperDbContext.BankAccounts.Select(ba => ba.FromEf()).ToList();
    }

    public async Task<List<Deposit>> GetAllDeposits()
    {
        return keeperDbContext.Deposits.Select(d => d.FromEf()).ToList();
    }

    public async Task<List<PayCard>> GetAllPayCards()
    {
        return keeperDbContext.PayCards.Select(pc => pc.FromEf()).ToList();
    }
}
