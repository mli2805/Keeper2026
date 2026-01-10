using KeeperDomain;

namespace KeeperInfrastructure;

public class AccountRepository(KeeperDbContext keeperDbContext)
{
    public async Task<List<Account>> GetAllAccounts()
    {
        return keeperDbContext.Accounts.Select(a=>a.FromEf()).ToList();
    }

    public async Task<List<BankAccount>> GetAllBankAccounts()
    {
        return keeperDbContext.BankAccounts.Select(ba=>ba.FromEf()).ToList();
    }

    public async Task<List<Deposit>> GetAllDeposits()
    {
        return keeperDbContext.Deposits.Select(d=>d.FromEf()).ToList();
    }

    public async Task<List<PayCard>> GetAllPayCards()
    {
        return keeperDbContext.PayCards.Select(pc=>pc.FromEf()).ToList();
    }
}
