using KeeperDomain;
using KeeperModels;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace KeeperInfrastructure;

public class AccountRepository(IDbContextFactory<KeeperDbContext> factory)
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
        var acMoDict = accountPlaneList.Select(a => a.ToModel()).ToDictionary(a => a.Id, a => a);
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

    public async Task<List<Account>> GetAllAccounts()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.Accounts.Select(a => a.FromEf()).ToList();
    }

    public async Task<List<BankAccountModel>> GetAllBankAccountsFromDb()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.BankAccounts.Select(ba => ba.ToModel()).ToList();
    }

    public async Task<List<Deposit>> GetAllDeposits()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.Deposits.Select(d => d.FromEf()).ToList();
    }

    public async Task<List<PayCard>> GetAllPayCards()
    {
        using var keeperDbContext = factory.CreateDbContext();
        return keeperDbContext.PayCards.Select(pc => pc.FromEf()).ToList();
    }

    public async Task Add(AccountItemModel accountItemModel)
    {
        using var keeperDbContext = factory.CreateDbContext();

        if (accountItemModel.IsBankAccount)
        {
            var bankAccountEf = accountItemModel.BankAccount!.FromModel();
            await keeperDbContext.BankAccounts.AddAsync(bankAccountEf);

            if (accountItemModel.IsDeposit)
            {
                var depositEf = accountItemModel.BankAccount!.Deposit!.ToEf();
                await keeperDbContext.Deposits.AddAsync(depositEf);
            }
            if (accountItemModel.IsCard)
            {
                var payCardEf = accountItemModel.BankAccount!.PayCard!.ToEf();
                await keeperDbContext.PayCards.AddAsync(payCardEf);
            }
        }

        var accountEf = accountItemModel.FromModel();
        await keeperDbContext.Accounts.AddAsync(accountEf);
        await keeperDbContext.SaveChangesAsync();
    }

    // обновление иерархии аккаунтов (ParentId, ChildNumber, IsExpanded) после перетаскивания в дереве
    public async Task UpdateTree(List<Account> accounts)
    {
        using var keeperDbContext = factory.CreateDbContext();
        foreach (var account in accounts)
        {
            var accountEf = keeperDbContext.Accounts.First(a => a.Id == account.Id);
            if (account.ParentId != 0)
                accountEf.ParentId = account.ParentId;
            accountEf.ChildNumber = account.ChildNumber;
            accountEf.IsExpanded = account.IsExpanded;
        }
        var d = await keeperDbContext.SaveChangesAsync();
    }

    public async Task Update(AccountItemModel accountItemModel)
    {
        using var keeperDbContext = factory.CreateDbContext();
        if (accountItemModel.IsBankAccount)
        {
            var bankAccountEf = keeperDbContext.BankAccounts.First(ba => ba.Id == accountItemModel.Id);
            bankAccountEf.UpdateFromModel(accountItemModel.BankAccount!);
            if (accountItemModel.IsDeposit)
            {
                var depositEf = keeperDbContext.Deposits.First(d => d.Id == accountItemModel.Id);
                depositEf.UpdateFromModel(accountItemModel.BankAccount!.Deposit!);
            }
            if (accountItemModel.IsCard)
            {
                var payCardEf = keeperDbContext.PayCards.First(c => c.Id == accountItemModel.Id);
                payCardEf.UpdateFromModel(accountItemModel.BankAccount!.PayCard!);
            }
        }
        var accountEf = keeperDbContext.Accounts.First(a => a.Id == accountItemModel.Id);
        accountEf.UpdateFromModel(accountItemModel);
        await keeperDbContext.SaveChangesAsync();
    }

    public async Task DeleteByAccountId(int id)
    {
        using var keeperDbContext = factory.CreateDbContext();
        await keeperDbContext.Accounts.Where(a => a.Id == id).ExecuteDeleteAsync();
        // если это не банк. счет, то никаких проблем не будет, просто ничего не удалится
        await keeperDbContext.BankAccounts.Where(ba => ba.Id == id).ExecuteDeleteAsync();
        await keeperDbContext.Deposits.Where(d => d.Id == id).ExecuteDeleteAsync();
        await keeperDbContext.PayCards.Where(p => p.Id == id).ExecuteDeleteAsync();
    }
}
